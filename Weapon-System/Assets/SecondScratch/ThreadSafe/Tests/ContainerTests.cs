using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Operations.Common;
using SecondScratch.ThreadSafe.Schedulers;
using SecondScratch.ThreadSafe.Schedulers.Implementations;
using SecondScratch.ThreadSafe.Tests.Mocks.Containers;
using SecondScratch.ThreadSafe.Tests.Mocks.Extensions;
using SecondScratch.ThreadSafe.Tests.Mocks.Operation;
using UnityEngine;
using UnityEngine.TestTools;

namespace SecondScratch.ThreadSafe.Tests
{
    public class ContainerTests
    {
        private const int CreateCapacity = ItemsCount / CreatesCount * ItemsSize;
        private const int CreatesCount = 2;

        private const int WarehouseCapacity = ItemsCount * ItemsSize;

        private const int ItemsSize = 3;
        private const int ItemsCount = 10 * CreatesCount;

        private const int PlayersCount = 3;
        private const int PlayerInventoryCapacity = WarehouseCapacity / PlayersCount;

        private Container _warehouse;
        private Container[] _creates;
        private Player[] _players;
        private Container[] _playerInventories;
        private Item[] _allItems;

        private Container[] _allContainers;

        private SimpleOperationMiddlewareSource _middlewareSource;
        private ICommandScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            _warehouse = new Container(WarehouseCapacity);

            _creates = new Container[CreatesCount];
            for (var i = 0; i < _creates.Length; i++)
                _creates[i] = new Container(CreateCapacity);

            _allItems = new Item[ItemsCount];
            for (var i = 0; i < ItemsCount; i++)
            {
                _allItems[i] = new Item(ItemsSize);
                _warehouse.Add(_allItems[i]);
            }

            _players = new Player[PlayersCount];
            _playerInventories = new Container[PlayersCount];
            for (var i = 0; i < PlayersCount; i++)
            {
                _players[i] = new Player(i);
                _playerInventories[i] = new Container(PlayerInventoryCapacity, i);
            }
            
            _allContainers = _playerInventories.Concat(_creates).Concat(new[] { _warehouse }).ToArray();

            _middlewareSource = new SimpleOperationMiddlewareSource();

            _scheduler = new ChannelCommandScheduler();
            _scheduler.RunConsumers();
            CommandScheduler.SetScheduler(_scheduler);
        }

        [Test]
        public async Task EveryoneMovesEveryReachableItemConcurrently_NoDataLost()
        {
            var attempts = new List<Attempt>();
            var handlers = new List<IOperationHandler>();

            for (var i = 0; i < _players.Length; i++)
            {
                var player = _players[i];
                for (var j = 0; j < _allItems.Length; j++)
                {
                    var item = _allItems[j];
                    var to = ChooseDestination(i, j);

                    attempts.Add(new Attempt(item, to));
                    
                    var handler = CreateOperationHandler(player, _warehouse, to, item);
                    handlers.Add(handler);
                }
            }

            var schedulerFailures = 0;

            void CountSchedulerFailure(string condition, string stackTrace, LogType type)
            {
                if (type == LogType.Exception && condition.StartsWith(nameof(SubjectMissingChildException)))
                    Interlocked.Increment(ref schedulerFailures);
            }

            Application.logMessageReceivedThreaded += CountSchedulerFailure;
            LogAssert.ignoreFailingMessages = true;

            try
            {
                var exceptions = await handlers.WhenAll();

                for (var i = 0; i < attempts.Count; i++)
                    attempts[i].Outcome = handlers[i].State;

                var committed = attempts.Count(a => a.Outcome == OperationState.Committed);
                var denied = attempts.Count(a => a.Outcome != OperationState.Committed);
                Debug.Log($"[Test] {attempts.Count} attempts: {committed} committed, {denied} denied, {exceptions} errors.");

                AssertIntegrity();

                foreach (var attempt in attempts.Where(a => a.Outcome == OperationState.Committed))
                    Assert.IsTrue(attempt.To.Children.Contains(attempt.Item),
                        $"Committed move of '{attempt.Item}' did not reach '{attempt.To}'.");

                Assert.AreEqual(denied, exceptions,
                    $"Expected {denied} failed operations for {denied} denied attempts, " +
                    $"but got {exceptions}.");
                Assert.AreEqual(denied, schedulerFailures,
                    $"Expected {denied} scheduler failure logs for {denied} denied attempts, " +
                    $"but got {schedulerFailures}.");
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
                Application.logMessageReceivedThreaded -= CountSchedulerFailure;
            }
        }

        [Test]
        public async Task ParallelTransfersOnManyThreads_NoDataLossOrOverflow()
        {
            var handlers = new List<IOperationHandler>();
            for (var i = 0; i < _allItems.Length; i++)
            {
                var item = _allItems[i];
                var from = _warehouse;
                var to = ChooseDestination(0, i);

                var handler = CreateOperationHandler(_players[0], from, to, item);
                handlers.Add(handler);
            }

            await handlers.WhenAll();

            AssertIntegrity();
        }

        private void AssertIntegrity()
        {
            var itemsCount = _allContainers.Select(c => c.Children.Count).Sum();
            Assert.AreEqual(_allItems.Length, itemsCount, "Lost an item");
            
            foreach (var container in _allContainers)
                Assert.LessOrEqual(container.UsedCapacity, container.Capacity, $"'{container}' exceeded capacity.");
            
            var uniqueItems = _allContainers.SelectMany(o => o.Children).Distinct().Count();
            Assert.AreEqual(itemsCount, uniqueItems, "Item duplication");
        }

        private Container ChooseDestination(int playerIndex, int itemIndex)
        {
            var inventory = _playerInventories[playerIndex];
            var item = _allItems[itemIndex];
            var destinations = _creates.Concat(new[] { inventory }).ToArray();
            var fits = destinations.Where(container => container.FreeCapacity >= item.Size).ToList();
            return fits.Count > 0 ? fits[(itemIndex + playerIndex) % fits.Count] : _warehouse;
        }

        private IOperationHandler CreateOperationHandler(Player player, Container from, Container to, Item item)
        {
            var context = new TransferContext(player, from, to, item);
            var removeOperation = ContainerRemoveOperation.Create(context);
            var addOperation = ContainerAddOperation.Create(context);
            return new OperationHandler<TransferContext>(_middlewareSource, context, removeOperation, addOperation);
        }

        private sealed class Attempt
        {
            public Item Item;
            public Container To;
            public OperationState Outcome;

            public Attempt(Item item, Container to)
            {
                Item = item;
                To = to;
            }
        }
    }
}