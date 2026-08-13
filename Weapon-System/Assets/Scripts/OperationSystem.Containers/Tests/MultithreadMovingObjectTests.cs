using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OperationSystem.Containers.Components;
using OperationSystem.Containers.Components.Containers;
using OperationSystem.Containers.Components.Containers.Locks.Accesses;
using OperationSystem.Containers.Components.Containers.Locks.Keys;
using OperationSystem.Containers.Middleware;
using OperationSystem.Containers.Operations;
using OperationSystem.Containers.Tests.Mocks;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.TestExtensions;
using OperationSystem.Units;
using UnityEngine;
using Random = System.Random;

namespace OperationSystem.Containers.Tests
{
    public class MultithreadMovingObjectTests
    {
        private const int Timeout = 1000;
        private const int UpdateDelay = 1;
        private const int RandomSeed = int.MaxValue / 727 / 7;
        
        [Test]
        public async Task CreateTest()
        {
            const int count = 100;

            var gameObject = new GameObject();
            var world = CreateWorld();

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
                assets[i] = gameObject.AddComponent<ContainerAsset>();
                assets[i].Set(items, containerLock, containerAccess);
                tasks[i] = Create(i);
            }

            await Task.WhenAll(tasks);
            world.Update();
            
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(i, assets[i].Items.Items.Count);
                Assert.AreEqual(keys[i], assets[i].KeyLock!.Value.Identifier);
                Assert.AreEqual(i, assets[i].AccessLock!.Value.Level);

                var unit = units[i];
                var unitItems = world.GetComponentArray<ContainerItems>().GetComponent(unit);
                var unitLock = world.GetComponentArray<KeyContainerLock>().GetComponent(unit);
                var unitAccess = world.GetComponentArray<AccessContainerLock>().GetComponent(unit);
                
                Assert.AreEqual(i, unitItems.Items.Count);
                Assert.AreEqual(keys[i], unitLock.Identifier);
                Assert.AreEqual(i, unitAccess.Level);
            }
            return;

            async Task Create(int index)
            {
                await Task.Delay(new Random(RandomSeed + index).Next(100));
                units[index] = world.GetOrCreateUnit(assets[index]);
            }
        }

        private static UnitWorld CreateWorld()
        {
            var middlewares = new IOperationMiddleware[]
            {
                new ContainerLockMiddleware(),
                new ContainerVolumeMiddleware()
            };
            
            var world = UnitWorld.Create();
            world.AppendMiddlewares(middlewares);
            return world;
        }

        [Test]
        public async Task ExecutionTest()
        {
            const int executorCount = 100;
            
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var targets = new Unit[executorCount];
            var containerAssets = new (ContainerAsset, ContainerAsset)[executorCount];
            var operations = new IOperation[executorCount];
            var tasks = new Task[executorCount];
            for (var i = 0; i < executorCount; i++)
            {
                var failOperation = i % 7 == 0;
                
                var targetAsset = gameObject.AddComponent<FooAsset>();
                targets[i] = world.GetOrCreateUnit(targetAsset);
                
                var keyIdentifier = Guid.NewGuid();

                var containerItems = ContainerItems.Create(targets[i]);
                var emptyItems = ContainerItems.Create();

                var firstItems = (i & 1) == 1 ? containerItems : emptyItems;
                var firstLock = new KeyContainerLock(keyIdentifier);
                var firstContainer = gameObject.AddComponent<ContainerAsset>();
                firstContainer.Set(firstItems, firstLock);
                
                var secondItems = (i & 1) == 1 ? emptyItems : containerItems;
                var secondLock = new AccessContainerLock(i);
                var secondContainer = gameObject.AddComponent<ContainerAsset>();
                secondContainer.Set(secondItems, null, secondLock);

                containerAssets[i] = (firstContainer, secondContainer);
            
                var key = new Key(keyIdentifier);
                var keyStorage = new KeysStorage(key);
                var access = failOperation ? new AccessLevel(-1) : new AccessLevel(i);
                var executorAsset = gameObject.AddComponent<ExecutorAsset>();
                executorAsset.Set(keyStorage, access);
                var executor = world.GetOrCreateUnit(executorAsset);

                var (senderAsset, receiverAsset) = (i & 1) == 1 
                    ? (firstContainer, secondContainer) 
                    : (secondContainer, firstContainer);
                
                (operations[i], tasks[i]) = RunOperation(executor, targets[i], senderAsset, receiverAsset, world);
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
        }
        
        private static (IOperation, Task) RunOperation(
            Unit executor,
            Unit target, 
            ContainerAsset sender, 
            ContainerAsset receiver,
            UnitWorld unitWorld)
        {
            var executorData = new OperationExecutor(executor);
            var targetData = new OperationTarget(target);
            
            var senderUnit = unitWorld.GetOrCreateUnit(sender);
            var receiverUnit = unitWorld.GetOrCreateUnit(receiver);
            
            var operation = new MovingObjectOperation(OperationIdentifier.CreateNew(), executorData, targetData, 
                senderUnit, receiverUnit);
            
            var task = operation.RunOperationAsTask(unitWorld, Timeout, UpdateDelay);
            return (operation, task);
        }
    }
}