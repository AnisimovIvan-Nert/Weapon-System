using System;
using System.Linq;
using System.Runtime.ExceptionServices;
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

namespace OperationSystem.Containers.Tests
{
    public class MovingObjectTests
    {
        private const int Timeout = 100;
        
        [Test]
        public void SimplePassTest()
        {
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var targetAsset = gameObject.AddComponent<FooAsset>();
            var target = world.GetOrCreateUnit(targetAsset);
            
            var sender = gameObject.AddComponent<ContainerAsset>();
            sender.Set(children: targetAsset);
            
            var receiver = gameObject.AddComponent<ContainerAsset>();
            
            var executorAsset = gameObject.AddComponent<ExecutorAsset>();
            var executor = world.GetOrCreateUnit(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertPass(operation, sender, receiver, targetAsset);
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
        public void SenderLockFailTest()
        {
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var targetAsset = gameObject.AddComponent<FooAsset>();
            var target = world.GetOrCreateUnit(targetAsset);
            
            var keyIdentifier = Guid.NewGuid();
            
            var senderLock = new KeyContainerLock(keyIdentifier);
            var sender = gameObject.AddComponent<ContainerAsset>();
            sender.Set(senderLock, children: targetAsset);
            
            var receiver = gameObject.AddComponent<ContainerAsset>();
            
            var executorAsset = gameObject.AddComponent<ExecutorAsset>();
            var executor = world.GetOrCreateUnit(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertFail(operation, sender, receiver, targetAsset);
        }
        
        [Test]
        public void SenderLockPassTest()
        {
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var targetAsset = gameObject.AddComponent<FooAsset>();
            var target = world.GetOrCreateUnit(targetAsset);
            
            var keyIdentifier = Guid.NewGuid();
            
            var senderLock = new KeyContainerLock(keyIdentifier);
            var sender = gameObject.AddComponent<ContainerAsset>();
            sender.Set(senderLock, children: targetAsset);
            
            var receiver = gameObject.AddComponent<ContainerAsset>();

            var key = new Key(keyIdentifier);
            var keyStorage = new KeysStorage(key);
            var executorAsset = gameObject.AddComponent<ExecutorAsset>();
            executorAsset.Set(keyStorage);
            var executor = world.GetOrCreateUnit(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertPass(operation, sender, receiver, targetAsset);
        }
        
        [Test]
        public void ReceiverLockFailTest()
        {
            const int accessLevel = 2;
            
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var targetAsset = gameObject.AddComponent<FooAsset>();
            var target = world.GetOrCreateUnit(targetAsset);
            
            var sender = gameObject.AddComponent<ContainerAsset>();
            sender.Set(children: targetAsset);
            
            var receiverLock = new AccessContainerLock(accessLevel);
            var receiver = gameObject.AddComponent<ContainerAsset>();
            receiver.Set(null, receiverLock);
            
            var access = new AccessLevel(accessLevel - 1);
            var executorAsset = gameObject.AddComponent<ExecutorAsset>();
            executorAsset.Set(null, access);
            var executor = world.GetOrCreateUnit(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertFail(operation, sender, receiver, targetAsset);
        }
        
        [Test]
        public void ReceiverLockPassTest()
        {
            const int accessLevel = 2;
            
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var targetAsset = gameObject.AddComponent<FooAsset>();
            var target = world.GetOrCreateUnit(targetAsset);
            
            var senderLock =  new AccessContainerLock(accessLevel + 1);
            var sender = gameObject.AddComponent<ContainerAsset>();
            sender.Set(null, senderLock, children: targetAsset);
            
            var receiverLock = new AccessContainerLock(accessLevel);
            var receiver = gameObject.AddComponent<ContainerAsset>();
            receiver.Set(null, receiverLock);
            
            var access = new AccessLevel(accessLevel + 1);
            var executorAsset = gameObject.AddComponent<ExecutorAsset>();
            executorAsset.Set(null, access);
            var executor = world.GetOrCreateUnit(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertPass(operation, sender, receiver, targetAsset);
        }
        
        [Test]
        public void VolumeFailTest()
        {
            const int height = 5;
            const int width = 10;
            const int volume = height * width;
            
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var size = new Size(height, width);
            var targetAsset = gameObject.AddComponent<FooAsset>();
            targetAsset.Set(size);
            var target = world.GetOrCreateUnit(targetAsset);
            
            var sender = gameObject.AddComponent<ContainerAsset>();
            sender.Set(children: targetAsset);
            
            var receiverVolume = new ContainerVolume(volume - 1);
            var receiver = gameObject.AddComponent<ContainerAsset>();
            receiver.Set(null, null, receiverVolume);
            
            var executorAsset = gameObject.AddComponent<ExecutorAsset>();
            var executor = world.GetOrCreateUnit(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertFail(operation, sender, receiver, targetAsset);
        }
        
        [Test]
        public void VolumePassTest()
        {
            const int height = 5;
            const int width = 10;
            const int volume = height * width;
            
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var size = new Size(height, width);
            var targetAsset = gameObject.AddComponent<FooAsset>();
            targetAsset.Set(size);
            var target = world.GetOrCreateUnit(targetAsset);
            
            var sender = gameObject.AddComponent<ContainerAsset>();
            sender.Set(children: targetAsset);
            
            var receiverVolume = new ContainerVolume(volume);
            var receiver = gameObject.AddComponent<ContainerAsset>();
            receiver.Set(null, null, receiverVolume);
            
            var executorAsset = gameObject.AddComponent<ExecutorAsset>();
            var executor = world.GetOrCreateUnit(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertPass(operation, sender, receiver, targetAsset);
        }
        
        private static IOperation RunAndWaitOperation(
            Unit executor,
            Unit target, 
            ContainerAsset sender, 
            ContainerAsset receiver,
            UnitWorld unitWorld)
        {
            var senderUnit = unitWorld.GetOrCreateUnit(sender);
            var receiverUnit = unitWorld.GetOrCreateUnit(receiver);

            var executorData = new OperationExecutor(executor);
            var targetData = new OperationTarget(target);
            
            var operation = new MovingObjectOperation(OperationIdentifier.CreateNew(), executorData, targetData, 
                senderUnit, receiverUnit);
            
            operation.RunOperationOnWorld(unitWorld);
            unitWorld.UpdateUntilComplete(operation, Timeout);

            return operation;
        }

        private static void AssertPass(
            IOperation operation, 
            ContainerAsset sender,
            ContainerAsset receiver,
            FooAsset target)
        {
            if (operation.Exception != null)
                ExceptionDispatchInfo.Capture(operation.Exception).Throw();
            
            operation.AssertPass();
            Assert.IsFalse(sender.Children.Contains(target));
            Assert.IsTrue(receiver.Children.Contains(target));
        }
        
        private static void AssertFail(
            IOperation operation, 
            ContainerAsset sender,
            ContainerAsset receiver,
            FooAsset target)
        {
            operation.AssertFail();
            Assert.IsTrue(sender.Children.Contains(target));
            Assert.IsFalse(receiver.Children.Contains(target));
        }
    }
}