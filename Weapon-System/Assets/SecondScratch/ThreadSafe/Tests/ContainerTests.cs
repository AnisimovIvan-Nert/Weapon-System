using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Tests.Mocks.Containers;
using SecondScratch.ThreadSafe.Tests.Mocks.Extensions;
using UnityEngine;

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
        }

        [Test]
        public async Task EveryoneMovesEveryReachableItemConcurrently_NoDataLost()
        {
            var attempts = new List<Attempt>();
            var interactions = new List<Operation<TransferContext>>();

            for (var i = 0; i < _players.Length; i++)
            {
                var player = _players[i];
                for (var j = 0; j < _allItems.Length; j++)
                {
                    var item = _allItems[j];
                    var destination = ChooseDestination(i, j);

                    attempts.Add(new Attempt(item, destination));

                    var interaction = TransferOperation.Create(CancellationToken.None);
                    interaction.Context.Player = player;
                    interaction.Context.From = _warehouse;
                    interaction.Context.To = destination;
                    interaction.Context.Item = item;
                    interactions.Add(interaction);
                }
            }

            var exceptions = await interactions.WhenAll();

            for (var i = 0; i < attempts.Count; i++)
                attempts[i].Outcome = interactions[i].State;

            var committed = attempts.Count(a => a.Outcome == InteractionState.Committed);
            var denied = attempts.Count(a => a.Outcome != InteractionState.Committed);
            Debug.Log($"[Test] {attempts.Count} attempts: {committed} committed, {denied} denied, {exceptions} errors.");

            AssertIntegrity();

            foreach (var attempt in attempts.Where(a => a.Outcome == InteractionState.Committed))
                Assert.IsTrue(attempt.To.Contains(attempt.Item),
                    $"Committed move of '{attempt.Item}' did not reach '{attempt.To}'.");

            Assert.AreEqual(denied, exceptions,
                $"Expected {denied} scheduler failure logs for {denied} denied attempts, " +
                $"but got {exceptions}.");
        }

        [Test]
        public async Task ParallelTransfersOnManyThreads_NoDataLossOrOverflow()
        {
            var operations = new List<IOperation>();
            for (var i = 0; i < _allItems.Length; i++)
            {
                var item = _allItems[i];
                var from = _warehouse;
                var to = ChooseDestination(0, i);

                var operation = TransferOperation.Create(CancellationToken.None);
                operation.Context.Player = _players[0];
                operation.Context.From = from;
                operation.Context.To = to;
                operation.Context.Item = item;

                operations.Add(operation);
            }

            await operations.WhenAll();

            AssertIntegrity();
        }

        private void AssertIntegrity()
        {
            var itemsCount = _allContainers.Select(c => c.Items.Count).Sum();
            Assert.AreEqual(_allItems.Length, itemsCount, "Lost an item");
            
            foreach (var container in _allContainers)
                Assert.LessOrEqual(container.UsedCapacity, container.Capacity, $"'{container}' exceeded capacity.");
            
            var uniqueItems = _allContainers.SelectMany(o => o.Items).Distinct().Count();
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

        private sealed class Attempt
        {
            public Item Item;
            public Container To;
            public InteractionState Outcome;

            public Attempt(Item item, Container to)
            {
                Item = item;
                To = to;
            }
        }
    }
}