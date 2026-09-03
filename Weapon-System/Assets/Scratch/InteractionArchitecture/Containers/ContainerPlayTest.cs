using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

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

        [SetUp]
        public void SetUp()
        {
            _world = new InteractionWorld();
            _world.RegisterThread("Main");

            // ---- Containers: capacity-limited, some owner-locked ---------
            _warehouse = new Container(0, "Warehouse", 20, null);
            _crateA = new Container(1, "Crate A", 10, 1001);
            _crateB = new Container(2, "Crate B", 10, 1002);

            // ---- Items ----
            var items = new[]
            {
                new Item(1, "Axe",   size: 3),
                new Item(2, "Log",   size: 2),
                new Item(3, "Rope",  size: 1),
                new Item(4, "Chest", size: 6),
                new Item(5, "Torch", size: 1)
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

            // ---- Players with their own private bags ----
            _players = new List<Player>
            {
                new Player(1001, "Alice", new Container(10, "Alice's bag", 8, 1001)),
                new Player(1002, "Bob",   new Container(11, "Bob's bag", 8, 1002)),
                new Player(1003, "Carol", new Container(12, "Carol's bag", 8, 1003)),
            };
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
            _world = null;
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

            // Drain the scheduler fully.
            Drain();

            // Record outcomes.
            for (var i = 0; i < attempts.Count; i++)
                attempts[i].Outcome = interactions[i].State;

            var committed = attempts.Count(a => a.Outcome == InteractionState.Committed);
            var failed = attempts.Count(a => a.Outcome == InteractionState.Failed);
            UnityEngine.Debug.Log(
                $"[PlayTest] {attempts.Count} attempts: {committed} committed, {failed} denied, " +
                $"{attempts.Count - committed - failed} rolled back.");

            // ---- The invariants that must always hold ---------------

            // 1. No item was created or destroyed by any (including rolled back) transfer.
            var present = containers.Sum(c => c.Slots.Count);
            Assert.AreEqual(_allSlots.Count, present,
                "Every item must still exist across all containers after concurrent moves.");

            // 2. No container ever exceeds its capacity.
            foreach (var container in containers)
                Assert.LessOrEqual(container.UsedCapacity, container.Capacity,
                    $"'{container}' exceeded capacity.");

            // 3. An item exists in exactly one place (no duplicated slot).
            var uniqueSlots = containers.SelectMany(c => c.Slots).Distinct().Count();
            Assert.AreEqual(present, uniqueSlots,
                "No slot may be present in two containers at once.");

            // 4. Committed moves really relocated the item (source no longer has it).
            foreach (var attempt in attempts.Where(a => a.Outcome == InteractionState.Committed))
                Assert.IsTrue(attempt.To.Contains(attempt.Slot),
                    $"Committed move of '{attempt.Slot.Item}' did not reach '{attempt.To}'.");
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
    }
}
