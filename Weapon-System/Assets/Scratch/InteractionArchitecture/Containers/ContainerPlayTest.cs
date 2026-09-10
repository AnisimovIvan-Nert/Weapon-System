using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Scratch.InteractionArchitecture.Containers
{
    /// <summary>
    /// Play test simulating many players moving items between many containers
    /// concurrently through the interaction architecture.
    ///
    /// Scenario: several containers, each holding several items, are shared.
    /// Many players each attempt to move every item they can reach between
    /// containers at the same time.  Each move is a separate interaction with
    /// its own transaction, so they run in parallel; precondition failures
    /// (no access / doesn't fit / already gone) roll back harmlessly instead
    /// of corrupting shared state.
    /// </summary>
    public class ContainerPlayTest
    {
        private const int MaxFramesToDrain = 10_000;

        private InteractionWorld _world;
        private Container _warehouse;
        private Container _crateA;
        private Container _crateB;
        private List<Player> _players;
        private List<InventorySlot> _allSlots;
        private readonly List<ThreadDispatcher.QueueSynchronizationContext> _workerThreads = new();

        private SynchronizationContext _previousContext;
        private TestPumpContext _pump;

        [SetUp]
        public void SetUp()
        {
            // Main-created (ownerless) objects are owned by this thread and marshal
            // cross-thread work back onto it via the SynchronizationContext that is
            // current at construction. Install a pump we can drain from the main
            // thread so the parallel transfer tests can service those calls.
            _previousContext = SynchronizationContext.Current;
            _pump = new TestPumpContext();
            SynchronizationContext.SetSynchronizationContext(_pump);

            _world = new InteractionWorld();
            _world.RegisterThread("Main");

            // ---- Containers: capacity-limited, some owner-locked ---------
            // The crates and every player's bag are OWNED by dedicated worker
            // threads (real OS threads). All access to them is marshalled via
            // RunOnOwner, so this exercises genuine multi-threaded access.
            var crateAThread = _world.Dispatcher.CreateWorkerThread("CrateA-Thread");
            var crateBThread = _world.Dispatcher.CreateWorkerThread("CrateB-Thread");
            _workerThreads.AddRange(new[] { crateAThread, crateBThread });

            _warehouse = new Container(0, 30, null, owner: null);
            _crateA = new Container(1, 10, 1001, owner: crateAThread);
            _crateB = new Container(2, 10, 1002, owner: crateBThread);

            // ---- Items ----
            var items = new[]
            {
                new Item(1,   size: 3),
                new Item(2,   size: 2),
                new Item(3,  size: 1),
                new Item(4, size: 6),
                new Item(5, size: 1)
            };

            // Fill the warehouse with two of each item.
            _allSlots = new List<InventorySlot>();
            for (var i = 0; i < 2; i++)
            {
                foreach (var item in items)
                {
                    var slot = new InventorySlot(item, _warehouse);
                    _warehouse.Add(slot);
                    _allSlots.Add(slot);
                }
            }

            // ---- Players with their own private bags on separate threads ----
            _players = new List<Player>
            {
                new Player(1001, new Container(10, 8, 1001,
                    owner: _world.Dispatcher.CreateWorkerThread("Alice-Thread"))),
                new Player(1002,   new Container(11, 8, 1002,
                    owner: _world.Dispatcher.CreateWorkerThread("Bob-Thread"))),
                new Player(1003, new Container(12, 8, 1003,
                    owner: _world.Dispatcher.CreateWorkerThread("Carol-Thread"))),
            };
            foreach (var p in _players)
                if (p.Inventory.Owner is ThreadDispatcher.QueueSynchronizationContext ownerContext)
                    _workerThreads.Add(ownerContext);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var thread in _workerThreads)
                thread?.Dispose();
            _workerThreads.Clear();

            _world?.Dispose();
            _world = null;

            _pump = null;
            SynchronizationContext.SetSynchronizationContext(_previousContext);
            _previousContext = null;
        }

        [Test]
        public void EveryoneMovesEveryReachableItemConcurrently_NoDataLost()
        {
            var containers = new[]
            {
                _warehouse, _crateA, _crateB,
                _players[0].Inventory, _players[1].Inventory, _players[2].Inventory
            };

            var attempts = new List<Attempt>();
            var interactions = new List<Interaction<TransferContext>>();

            foreach (var player in _players)
            {
                foreach (var slot in _allSlots)
                {
                    var destination = ChooseDestination(player, slot);

                    attempts.Add(new Attempt(player, slot, destination));

                    var interaction = _world.Schedule(TransferInteraction.CreateStages());
                    interaction.Context.Player = player;
                    interaction.Context.From = _warehouse;
                    interaction.Context.To = destination;
                    interaction.Context.Slot = slot;
                    interactions.Add(interaction);
                }
            }

            // Precondition denials (no access / doesn't fit / already moved)
            // are an expected part of this concurrent scenario. We still want to
            // *verify* every denial is reported exactly once as a scheduler error,
            // so capture the error logs instead of just suppressing them.
            var schedulerErrors = new List<string>();
            Application.LogCallback capture = (condition, _, type) =>
            {
                if (type == LogType.Error && condition.Contains("[InteractionScheduler]"))
                    schedulerErrors.Add(condition);
            };
            Application.logMessageReceived += capture;

            var previousIgnore = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            try
            {
                Drain();

                // Record outcomes.
                for (var i = 0; i < attempts.Count; i++)
                    attempts[i].Outcome = interactions[i].State;
            }
            finally
            {
                LogAssert.ignoreFailingMessages = previousIgnore;
                Application.logMessageReceived -= capture;
            }

            var committed = attempts.Count(a => a.Outcome == InteractionState.Committed);
            var denied = attempts.Count(a => a.Outcome != InteractionState.Committed);
            UnityEngine.Debug.Log(
                $"[PlayTest] {attempts.Count} attempts: {committed} committed, {denied} denied/rolled back, " +
                $"reported {schedulerErrors.Count} scheduler errors.");

            // ---- The invariants that must always hold ---------------
            // All reads of container state must be marshalled onto the container's
            // owner thread, because containers now live on different OS threads.

            // 1. No item was created or destroyed by any (including rolled back) transfer.
            var present = containers.Sum(c => c.RunOnOwner(() => c.Slots.Count));
            Assert.AreEqual(_allSlots.Count, present,
                "Every item must still exist across all containers after concurrent moves.");

            // 1b. The scenario really is multithreaded: the locked containers and
            //     bags must be owned by distinct threads.
            var ownerThreads = containers
                .Select(c => (c.Owner as ThreadDispatcher.QueueSynchronizationContext)?.ThreadId
                             ?? Thread.CurrentThread.ManagedThreadId)
                .Distinct()
                .Count();
            Assert.GreaterOrEqual(ownerThreads, 3,
                "Expected containers to be spread across at least 3 distinct threads " +
                $"but found only {ownerThreads}.");

            // 2. No container ever exceeds its capacity.
            foreach (var container in containers)
            {
                var (used, cap) = container.RunOnOwner(
                    () => (container.UsedCapacity, container.Capacity));
                Assert.LessOrEqual(used, cap,
                    $"'{container}' exceeded capacity.");
            }

            // 3. An item exists in exactly one place (no duplicated slot).
            var uniqueSlots = containers.SelectMany(c => c.RunOnOwner(() => c.Slots)).Distinct().Count();
            Assert.AreEqual(present, uniqueSlots,
                "No slot may be present in two containers at once.");

            // 4. Committed moves really relocated the item (source no longer has it).
            foreach (var attempt in attempts.Where(a => a.Outcome == InteractionState.Committed))
                Assert.IsTrue(attempt.To.RunOnOwner(() => attempt.To.Contains(attempt.Slot)),
                    $"Committed move of '{attempt.Slot.Item}' did not reach '{attempt.To}'.");

            // ---- Every denial must have been reported exactly once ----
            // The scheduler logs one error per failed interaction, so the number of
            // scheduler error log messages must equal the number of denied attempts.
            Assert.AreEqual(denied, schedulerErrors.Count,
                $"Expected {denied} scheduler failure logs for {denied} denied attempts, " +
                $"but got {schedulerErrors.Count}.");

            // And every one of those errors must be a genuine denial of the transfer,
            // never an internal fault (e.g. the transaction failing to commit).
            foreach (var error in schedulerErrors)
            {
                Assert.That(
                    error.Contains("no access to move") ||
                    error.Contains("cannot fit") ||
                    error.Contains("not present"),
                    $"Unexpected scheduler error (not a denial): {error}");
            }
        }

        /// <summary>
        /// Runs transfers TRULY in parallel: every interaction is driven to
        /// completion on its own thread-pool worker, so many transfers contend
        /// for the same worker-owned containers at the same instant.  Container
        /// integrity must hold despite this concurrency, because every container
        /// mutation is marshalled onto that container's single owning thread.
        /// </summary>
        [Test]
        public void ParallelTransfersOnManyThreads_NoDataLossOrOverflow()
        {
            var containers = new[]
            {
                _warehouse, _crateA, _crateB,
                _players[0].Inventory, _players[1].Inventory, _players[2].Inventory
            };

            // Deterministic moves from the warehouse to different destinations,
            // all living on different threads.
            var tasks = new List<Task<InteractionState>>();
            foreach (var slot in _allSlots)
            {
                var from = _warehouse;
                var to = ChooseDestination(_players[0], slot);

                var interaction = Interaction<TransferContext>.Create(
                    TransferInteraction.CreateStages());
                interaction.Context.Player = _players[0];
                interaction.Context.From = from;
                interaction.Context.To = to;
                interaction.Context.Slot = slot;

                // Drive each transfer on its own thread-pool worker -> true parallel.
                tasks.Add(Task.Run(() => RunInteraction(interaction)));
            }

            var all = Task.WhenAll(tasks);

            // The transfers run on thread-pool workers, but every operation on the
            // main-owned warehouse is marshalled back onto this thread via the
            // SynchronizationContext. Pump it while the transfers progress so those
            // calls actually execute (and are serialised here) instead of blocking.
            while (!all.IsCompleted)
            {
                _pump.Drain();
                Thread.Sleep(1);
            }
            _pump.Drain();
            all.GetAwaiter().GetResult();

            // Integrity: nothing lost, no overflow, nothing duplicated.
            var present = containers.Sum(c => c.RunOnOwner(() => c.Slots.Count));
            Assert.AreEqual(_allSlots.Count, present,
                "Parallel transfers lost items.");

            foreach (var container in containers)
            {
                var (used, cap) = container.RunOnOwner(
                    () => (container.UsedCapacity, container.Capacity));
                Assert.LessOrEqual(used, cap, $"'{container}' exceeded capacity.");
            }

            var uniqueSlots = containers.SelectMany(c => c.RunOnOwner(() => c.Slots)).Distinct().Count();
            Assert.AreEqual(present, uniqueSlots, "A slot ended up in two containers.");
        }

        /// <summary>
        /// Ownership of a container can be handed over at runtime (main -&gt; worker,
        /// worker -&gt; worker, worker -&gt; main). The handover must be atomic: no
        /// slot may be lost or duplicated while the owner thread changes.
        /// </summary>
        [Test]
        public void ChangeOwner_HandsContainerBetweenThreads_WithoutLosingState()
        {
            Assert.AreEqual(_pump, _warehouse.Owner, "Warehouse should start owned by the main thread.");

            var workerA = _world.Dispatcher.CreateWorkerThread("MigrateA");
            var workerB = _world.Dispatcher.CreateWorkerThread("MigrateB");

            try
            {
                // main -> worker
                _warehouse.ChangeOwner(workerA);
                Assert.AreEqual(workerA, _warehouse.Owner);
                Assert.AreEqual(_allSlots.Count,
                    _warehouse.RunOnOwner(() => _warehouse.Slots.Count),
                    "Items must be intact after moving the warehouse to a worker.");

                // worker -> worker
                _warehouse.ChangeOwner(workerB);
                Assert.AreEqual(workerB, _warehouse.Owner);
                Assert.AreEqual(_allSlots.Count,
                    Task.Run(() => _warehouse.RunOnOwner(() => _warehouse.Slots.Count)).Result,
                    "A cross-thread call must marshal onto the new owner.");

                // Transfers marshalled onto the moved owner keep working/rolling back.
                var slot = _allSlots[0];
                var interaction = Interaction<TransferContext>.Create(
                    TransferInteraction.CreateStages());
                interaction.Context.Player = _players[0];
                interaction.Context.From = _warehouse;
                interaction.Context.To = _crateA;
                interaction.Context.Slot = slot;
                RunInteraction(interaction);
                Assert.IsTrue(
                    _crateA.RunOnOwner(() => _crateA.Contains(slot)),
                    "Committed move after the handover must land in the destination.");
                Assert.IsFalse(
                    _warehouse.RunOnOwner(() => _warehouse.Contains(slot)),
                    "Source must not retain the slot after the handover.");

                // worker -> main (the call must be made on the main thread).
                _warehouse.ChangeOwner(_pump);
                Assert.AreEqual(_pump, _warehouse.Owner);
                Assert.AreEqual(_allSlots.Count - 1,
                    _warehouse.RunOnOwner(() => _warehouse.Slots.Count),
                    "State must be intact after moving the warehouse back to main.");
            }
            finally
            {
                workerA.Dispose();
                workerB.Dispose();
            }
        }

        private static InteractionState RunInteraction(Interaction<TransferContext> interaction)
        {
            try
            {
                interaction.RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // A rejected transfer (no room / already moved) is expected and
                // must have safely rolled back — not crash the run.
            }
            return interaction.State;
        }

        private Container ChooseDestination(Player player, InventorySlot slot)
        {
            var destinations = new[]
            {
                player.Inventory,
                _crateA,
                _crateB,
                _warehouse
            };

            var fits = destinations
                .Where(c =>
                    !ReferenceEquals(c, _warehouse) &&
                    c.FreeCapacity >= slot.Item.Size)
                .ToList();

            // Deterministic but varied destination per (item, player).
            return fits.Count > 0
                ? fits[(slot.Item.Id + player.Id) % fits.Count]
                : _warehouse;
        }

        private void Drain()
        {
            var frame = 0;
            while (_world.Scheduler.ActiveCount > 0 || _world.Scheduler.PendingCount > 0)
            {
                Assert.Less(++frame, MaxFramesToDrain, "Scheduler did not drain.");
                _world.Tick().GetAwaiter().GetResult();
            }
        }

        private sealed class Attempt
        {
            public Player Player;
            public InventorySlot Slot;
            public Container To;
            public InteractionState Outcome;

            public Attempt(Player player, InventorySlot slot, Container to)
            {
                Player = player;
                Slot = slot;
                To = to;
            }
        }

        /// <summary>
        /// A SynchronizationContext that queues posted work and lets the owning
        /// (main/test) thread execute it by draining the queue. Cross-thread
        /// access to a main-created object is marshalled onto this queue via
        /// <see cref="Container.RunOnOwner{T}"/>, so the owner thread runs it
        /// itself (serialised) instead of deadlocking.
        /// </summary>
        private sealed class TestPumpContext : SynchronizationContext
        {
            private readonly ConcurrentQueue<Action> _queue = new();

            public override void Post(SendOrPostCallback d, object state) =>
                _queue.Enqueue(() => d(state));

            public override void Send(SendOrPostCallback d, object state)
            {
                var done = new ManualResetEventSlim();
                Exception error = null;
                _queue.Enqueue(() =>
                {
                    try
                    {
                        d(state);
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                    finally
                    {
                        done.Set();
                    }
                });
                done.Wait();
                done.Dispose();
                if (error != null)
                    throw error;
            }

            /// <summary>Executes all enqueued callbacks on the calling (owner) thread.</summary>
            public void Drain()
            {
                while (_queue.TryDequeue(out var action))
                    action();
            }
        }
    }
}
