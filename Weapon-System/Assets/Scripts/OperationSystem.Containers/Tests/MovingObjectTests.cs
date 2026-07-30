using System;
using System.Runtime.ExceptionServices;
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
using OperationSystem.Units;

namespace OperationSystem.Containers.Tests
{
    public class MovingObjectTests
    {
        private const int Timeout = 100;
        
        [Test]
        public void SimplePassTest()
        {
            var world = UnitWorld.Create();
            
            var targetAsset = new FooAsset();
            var target = world.Registry.Create(targetAsset);
            
            var senderItems = ContainerItems.Create(target);
            var sender = new ContainerAsset(senderItems);

            var receiverItems = ContainerItems.Create();
            var receiver = new ContainerAsset(receiverItems);
            
            var executorAsset = new ExecutorAsset();
            var executor = world.Registry.Create(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertPass(operation, sender, receiver, target);
        }
        
        [Test]
        public void SenderLockFailTest()
        {
            var world = UnitWorld.Create();
            
            var targetAsset = new FooAsset();
            var target = world.Registry.Create(targetAsset);
            
            var keyIdentifier = Guid.NewGuid();
            
            var senderItems = ContainerItems.Create(target);
            var senderLock = new KeyContainerLock(keyIdentifier);
            var sender = new ContainerAsset(senderItems, senderLock);

            var receiverItems = ContainerItems.Create();
            var receiver = new ContainerAsset(receiverItems);
            
            var executorAsset = new ExecutorAsset();
            var executor = world.Registry.Create(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertFail(operation, sender, receiver, target);
        }
        
        [Test]
        public void SenderLockPassTest()
        {
            var world = UnitWorld.Create();
            
            var targetAsset = new FooAsset();
            var target = world.Registry.Create(targetAsset);
            
            var keyIdentifier = Guid.NewGuid();
            
            var senderItems = ContainerItems.Create(target);
            var senderLock = new KeyContainerLock(keyIdentifier);
            var sender = new ContainerAsset(senderItems, senderLock);

            var receiverItems = ContainerItems.Create();
            var receiver = new ContainerAsset(receiverItems);

            var key = new Key(keyIdentifier);
            var keyStorage = new KeysStorage(key);
            var executorAsset = new ExecutorAsset(keyStorage);
            var executor = world.Registry.Create(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertPass(operation, sender, receiver, target);
        }
        
        [Test]
        public void ReceiverLockFailTest()
        {
            const int accessLevel = 2;
            
            var world = UnitWorld.Create();
            
            var targetAsset = new FooAsset();
            var target = world.Registry.Create(targetAsset);
            
            var senderItems = ContainerItems.Create(target);
            var sender = new ContainerAsset(senderItems);

            var receiverItems = ContainerItems.Create();
            var receiverLock = new AccessContainerLock(accessLevel);
            var receiver = new ContainerAsset(receiverItems, null, receiverLock);
            
            var access = new AccessLevel(accessLevel - 1);
            var executorAsset = new ExecutorAsset(null, access);
            var executor = world.Registry.Create(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertFail(operation, sender, receiver, target);
        }
        
        [Test]
        public void ReceiverLockPassTest()
        {
            const int accessLevel = 2;
            
            var world = UnitWorld.Create();
            
            var targetAsset = new FooAsset();
            var target = world.Registry.Create(targetAsset);
            
            var senderItems = ContainerItems.Create(target);
            var senderLock =  new AccessContainerLock(accessLevel + 1);
            var sender = new ContainerAsset(senderItems, null, senderLock);

            var receiverItems = ContainerItems.Create();
            var receiverLock = new AccessContainerLock(accessLevel);
            var receiver = new ContainerAsset(receiverItems, null, receiverLock);
            
            var access = new AccessLevel(accessLevel + 1);
            var executorAsset = new ExecutorAsset(null, access);
            var executor = world.Registry.Create(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertPass(operation, sender, receiver, target);
        }
        
        [Test]
        public void VolumeFailTest()
        {
            const int height = 5;
            const int width = 10;
            const int volume = height * width;
            
            var world = UnitWorld.Create();
            
            var size = new Size(height, width);
            var targetAsset = new FooAsset(size);
            var target = world.Registry.Create(targetAsset);
            
            var senderItems = ContainerItems.Create(target);
            var sender = new ContainerAsset(senderItems);

            var receiverItems = ContainerItems.Create();
            var receiverVolume = new ContainerVolume(volume - 1);
            var receiver = new ContainerAsset(receiverItems, null, null, receiverVolume);
            
            var executorAsset = new ExecutorAsset();
            var executor = world.Registry.Create(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertFail(operation, sender, receiver, target);
        }
        
        [Test]
        public void VolumePassTest()
        {
            const int height = 5;
            const int width = 10;
            const int volume = height * width;
            
            var world = UnitWorld.Create();
            
            var size = new Size(height, width);
            var targetAsset = new FooAsset(size);
            var target = world.Registry.Create(targetAsset);
            
            var senderItems = ContainerItems.Create(target);
            var sender = new ContainerAsset(senderItems);

            var receiverItems = ContainerItems.Create();
            var receiverVolume = new ContainerVolume(volume);
            var receiver = new ContainerAsset(receiverItems, null, null, receiverVolume);
            
            var executorAsset = new ExecutorAsset();
            var executor = world.Registry.Create(executorAsset);

            var operation = RunAndWaitOperation(executor, target, sender, receiver, world);
            
            AssertPass(operation, sender, receiver, target);
        }
        
        private static IOperation RunAndWaitOperation(
            Unit executor,
            Unit target, 
            ContainerAsset sender, 
            ContainerAsset receiver,
            UnitWorld unitWorld)
        {
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
            
            var timeout = Timeout;
            while (!operation.IsCompleted && timeout > 0)
            {
                timeout--;
                globalHandler.Update();
                senderHandler.Update();
                receiverHandler.Update();
            }

            return operation;
        }

        private static void AssertPass(
            IOperation operation, 
            ContainerAsset sender,
            ContainerAsset receiver,
            Unit target)
        {
            Assert.IsTrue(operation.IsCompleted);
            
            if (operation.Exception != null)
                ExceptionDispatchInfo.Capture(operation.Exception).Throw();
            
            Assert.IsFalse(sender.Items.Items.Contains(target));
            Assert.IsTrue(receiver.Items.Items.Contains(target));

            Assert.IsTrue(operation.IsCompletedSuccessfully);
        }
        
        private static void AssertFail(
            IOperation operation, 
            ContainerAsset sender,
            ContainerAsset receiver,
            Unit target)
        {
            Assert.IsTrue(operation.IsCompleted);
            
            Assert.IsTrue(sender.Items.Items.Contains(target));
            Assert.IsFalse(receiver.Items.Items.Contains(target));
            
            Assert.IsFalse(operation.IsCompletedSuccessfully);
        }
    }
}