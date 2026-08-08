using System;
using System.Threading.Tasks;
using Coroutine;
using NUnit.Framework;
using OperationSystem.Containers.Components;
using OperationSystem.Containers.Components.Containers;
using OperationSystem.Containers.Components.Containers.Locks.Accesses;
using OperationSystem.Containers.Components.Containers.Locks.Keys;
using OperationSystem.Containers.Middleware;
using OperationSystem.Containers.Operations;
using OperationSystem.Containers.Tests.Mocks;
using OperationSystem.Containers.UnitHandlers;
using OperationSystem.Handlers;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Tests;
using OperationSystem.Units;
using UnityEngine;
using Random = System.Random;

namespace OperationSystem.Containers.Tests
{
    public class MultithreadMovingObjectTests
    {
        private const int Timeout = 1000;
        private const int RandomSeed = int.MaxValue / 727 / 7;
        
        [Test]
        public async Task CreateTest()
        {
            const int count = 100;
            
            var world = UnitWorld.Create();

            var assets = new ContainerAsset[count];
            var units = new Unit[count];
            var keys = new Guid[count];
            var tasks = new Task[count];
            for (var i = 0; i < count; i++)
            {
                keys[i] = Guid.NewGuid();
                var items = ContainerItems.Create(new Unit[i]);
                var containerLock = new KeyContainerLock(keys[i]);
                var containerAccess = new AccessContainerLock(i);
                assets[i] = new ContainerAsset(items, containerLock, containerAccess);
                tasks[i] = Create(i);
            }

            await Task.WhenAll(tasks);
            
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(i, assets[i].Items.Items.Count);
                Assert.AreEqual(keys[i], assets[i].KeyLock!.Value.Identifier);
                Assert.AreEqual(i, assets[i].AccessLock!.Value.Level);

                var unit = units[i];
                var unitItems = world.GetComponents<ContainerItems>().GetComponent(unit.Id);
                var unitLock = world.GetComponents<KeyContainerLock>().GetComponent(unit.Id);
                var unitAccess = world.GetComponents<AccessContainerLock>().GetComponent(unit.Id);
                
                Assert.AreEqual(i, unitItems.Items.Count);
                Assert.AreEqual(keys[i], unitLock.Identifier);
                Assert.AreEqual(i, unitAccess.Level);
            }
            return;

            async Task Create(int index)
            {
                await Task.Delay(new Random(RandomSeed + index).Next(100));
                
                var runner = new OperationRunner();
                var handler = new ContainerHandler(runner, world);
                handler.SetUnit(assets[index]).Wait();
                units[index] = handler.Unit ?? throw new InvalidOperationException();
                
                handler.Update();
                handler.Update();
                handler.Update();
                
                Debug.Log(index);
            }
        }
        
        [Test]
        public async Task ExecutionTest()
        {
            const int executorCount = 100;
            
            var world = UnitWorld.Create();
            
            var targets = new Unit[executorCount];
            var containerAssets = new (ContainerAsset, ContainerAsset)[executorCount];
            var operations = new IOperation[executorCount];
            var tasks = new Task[executorCount];
            for (var i = 0; i < executorCount; i++)
            {
                var failOperation = i % 7 == 0;
                
                var targetAsset = new FooAsset();
                targets[i] = world.Registry.Create(targetAsset);
                
                var keyIdentifier = Guid.NewGuid();

                var containerItems = ContainerItems.Create(targets[i]);
                var emptyItems = ContainerItems.Create();

                var firstItems = (i & 1) == 1 ? containerItems : emptyItems;
                var firstLock = new KeyContainerLock(keyIdentifier);
                var firstContainer = new ContainerAsset(firstItems, firstLock);
                
                var secondItems = (i & 1) == 1 ? emptyItems : containerItems;
                var secondLock = new AccessContainerLock(i);
                var secondContainer = new ContainerAsset(secondItems, null, secondLock);

                containerAssets[i] = (firstContainer, secondContainer);
            
                var key = new Key(keyIdentifier);
                var keyStorage = new KeysStorage(key);
                var access = failOperation ? new AccessLevel(-1) : new AccessLevel(i);
                var executorAsset = new ExecutorAsset(keyStorage, access);
                var executor = world.Registry.Create(executorAsset);

                var (senderAsset, receiverAsset) = (i & 1) == 1 
                    ? (firstContainer, secondContainer) 
                    : (secondContainer, firstContainer);

                var (operation, handler, senderHandler, receiverHandler) =
                    RunOperation(executor, targets[i], senderAsset, receiverAsset, world);
                operations[i] = operation;

                var mainTask = WaitOperation(i, handler, RandomSeed + i + 1 * 100);
                var senderTask = WaitOperation(i, senderHandler, RandomSeed + i + 2 * 100);
                var receiverTask = WaitOperation(i, receiverHandler, RandomSeed + i + 3 * 100);
                var whenAll = Task.WhenAll(mainTask, senderTask, receiverTask);
                tasks[i] = whenAll;
            }

            await Task.WhenAll(tasks);

            for (var i = 0; i < executorCount; i++)
            {
                var failOperation = i % 7 == 0;
                
                if (failOperation)
                    operations[i].AssertFail();
                else
                    operations[i].AssertPass();

                var senderAsset = (i & 1) == 1 ? containerAssets[i].Item1 : containerAssets[i].Item2;
                var receiverAsset = (i & 1) == 1 ? containerAssets[i].Item2 : containerAssets[i].Item1;
                var target = targets[i];

                if (failOperation)
                {
                    Assert.IsTrue(senderAsset.Items.Items.Contains(target));
                    Assert.IsFalse(receiverAsset.Items.Items.Contains(target));
                }
                else
                {
                    Assert.IsFalse(senderAsset.Items.Items.Contains(target));
                    Assert.IsTrue(receiverAsset.Items.Items.Contains(target));
                }
            }
            
            return;
            
            async Task WaitOperation(int index, IOperationHandler handler, int seed)
            {
                var timeout = Timeout;
                var operation = operations[index];
                var random = new Random(seed);
                while (!operation.IsCompleted && timeout > 0)
                {
                    await Task.Delay(random.Next(10));
                    timeout--;
                    handler.Update();
                }
                Debug.Log(index);
            }
        }
        
        private static (IOperation, OperationHandler, ContainerHandler, ContainerHandler) RunOperation(
            Unit executor,
            Unit target, 
            ContainerAsset sender, 
            ContainerAsset receiver,
            UnitWorld unitWorld)
        {
            unitWorld.PullFromAssets(executor);
            unitWorld.PullFromAssets(target);
            
            var middlewares = new IOperationMiddleware[]
            {
                new ContainerLockMiddleware(),
                new ContainerVolumeMiddleware()
            };

            var executorData = new OperationExecutor(executor);
            var targetData = new OperationTarget(target);
            
            var senderRunner = new OperationRunner();
            var senderHandler = new ContainerHandler(senderRunner, unitWorld);
            senderHandler.SetUnit(sender).Wait();
            
            var receiverRunner = new OperationRunner();
            var receiverHandler = new ContainerHandler(receiverRunner, unitWorld);
            receiverHandler.SetUnit(receiver).Wait();
            
            var globalRunner = new OperationRunner();
            var globalHandler = new OperationHandler(globalRunner, unitWorld);
            
            var operation = new MovingObjectOperation(OperationIdentifier.CreateNew(), executorData, targetData, 
                senderHandler, receiverHandler, middlewares);
            
            operation.RunOperation(globalHandler);

            return (operation, globalHandler, senderHandler, receiverHandler);
        }
    }
}