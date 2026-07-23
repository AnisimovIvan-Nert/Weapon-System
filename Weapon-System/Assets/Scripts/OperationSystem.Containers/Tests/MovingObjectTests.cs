using System;
using System.Runtime.ExceptionServices;
using Coroutine;
using NUnit.Framework;
using OperationSystem.Containers.Middleware;
using OperationSystem.Containers.Operations;
using OperationSystem.Containers.Tests.Mocks;
using OperationSystem.Containers.UnitHandlers;
using OperationSystem.Containers.Units;
using OperationSystem.Containers.Units.Containers;
using OperationSystem.Containers.Units.Containers.Locks.Accesses;
using OperationSystem.Containers.Units.Containers.Locks.Keys;
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
            var target = new FooUnit();

            var senderItems = new ContainerItems(target);
            var sender = new Container(senderItems);

            var receiverItems = new ContainerItems();
            var receiver = new Container(receiverItems);

            var executor = new Executor();

            var operation = RunAndWaitOperation(executor, target, sender, receiver);
            
            AssertPass(operation, senderItems, receiverItems, target);
        }
        
        [Test]
        public void SenderLockFailTest()
        {
            var target = new FooUnit();

            var keyIdentifier = Guid.NewGuid();
            
            var senderItems = new ContainerItems(target);
            var senderLock = new KeyContainerLock(keyIdentifier);
            var sender = new Container(senderItems, senderLock);

            var receiverItems = new ContainerItems();
            var receiver = new Container(receiverItems);

            var executor = new Executor();

            var operation = RunAndWaitOperation(executor, target, sender, receiver);
            
            AssertFail(operation, senderItems, receiverItems, target);
        }
        
        [Test]
        public void SenderLockPassTest()
        {
            var target = new FooUnit();

            var keyIdentifier = Guid.NewGuid();
            
            var senderItems = new ContainerItems(target);
            var senderLock = new KeyContainerLock(keyIdentifier);
            var sender = new Container(senderItems, senderLock);

            var receiverItems = new ContainerItems();
            var receiver = new Container(receiverItems);

            var key = new Key(keyIdentifier);
            var keysStorage = new KeysStorage(key);
            var executor = new Executor(keysStorage);

            var operation = RunAndWaitOperation(executor, target, sender, receiver);
            
            AssertPass(operation, senderItems, receiverItems, target);
        }
        
        [Test]
        public void ReceiverLockFailTest()
        {
            const int accessLevel = 2;
            
            var target = new FooUnit();
            
            var senderItems = new ContainerItems(target);
            var sender = new Container(senderItems);

            var receiverItems = new ContainerItems();
            var receiverLock = new AccessContainerLock(accessLevel);
            var receiver = new Container(receiverItems, receiverLock);

            var access = new AccessLevel(accessLevel - 1);
            var executor = new Executor(access);

            var operation = RunAndWaitOperation(executor, target, sender, receiver);
            
            AssertFail(operation, senderItems, receiverItems, target);
        }
        
        [Test]
        public void ReceiverLockPassTest()
        {
            const int accessLevel = 2;
            
            var target = new FooUnit();
            
            var senderItems = new ContainerItems(target);
            var senderLock =  new AccessContainerLock(accessLevel + 1);
            var sender = new Container(senderItems, senderLock);

            var receiverItems = new ContainerItems();
            var receiverLock = new AccessContainerLock(accessLevel);
            var receiver = new Container(receiverItems, receiverLock);

            var access = new AccessLevel(accessLevel + 1);
            var executor = new Executor(access);

            var operation = RunAndWaitOperation(executor, target, sender, receiver);
            
            AssertPass(operation, senderItems, receiverItems, target);
        }
        
        [Test]
        public void VolumeFailTest()
        {
            const int height = 5;
            const int width = 10;
            const int volume = height * width;

            var size = new Size(height, width);
            var target = new FooUnit(size);

            var senderItems = new ContainerItems(target);
            var sender = new Container(senderItems);

            var receiverItems = new ContainerItems();
            var receiverVolume = new ContainerVolume(volume - 1);
            var receiver = new Container(receiverItems, receiverVolume);

            var executor = new Executor();

            var operation = RunAndWaitOperation(executor, target, sender, receiver);
            
            AssertFail(operation, senderItems, receiverItems, target);
        }
        
        [Test]
        public void VolumePassTest()
        {
            const int height = 5;
            const int width = 10;
            const int volume = height * width;

            var size = new Size(height, width);
            var target = new FooUnit(size);

            var senderItems = new ContainerItems(target);
            var sender = new Container(senderItems);

            var receiverItems = new ContainerItems();
            var receiverVolume = new ContainerVolume(volume);
            var receiver = new Container(receiverItems, receiverVolume);

            var executor = new Executor();

            var operation = RunAndWaitOperation(executor, target, sender, receiver);
            
            AssertPass(operation, senderItems, receiverItems, target);
        }
        
        private static IOperation RunAndWaitOperation(
            IUnit executor,
            IUnit target, 
            IContainer sender, 
            IContainer receiver)
        {
            var middlewares = new IOperationMiddleware[]
            {
                new ContainerLockMiddleware(),
                new ContainerVolumeMiddleware()
            };

            var executorData = new OperationExecutor(executor);
            var targetData = new OperationTarget(target);
            
            var senderRunner = new OperationRunner();
            var senderHandler = new ContainerHandler(senderRunner);
            senderHandler.SetUnit(sender).Wait();
            
            var receiverRunner = new OperationRunner();
            var receiverHandler = new ContainerHandler(receiverRunner);
            receiverHandler.SetUnit(receiver).Wait();
            
            var globalRunner = new OperationRunner();
            var globalHandler = new OperationHandler(globalRunner);
            
            var guid = Guid.NewGuid();
            var operation = new MovingObjectOperation(guid, executorData, targetData, 
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
            ContainerItems sender,
            ContainerItems receiver,
            IUnit target)
        {
            Assert.IsTrue(operation.IsCompleted);
            
            if (operation.Exception != null)
                ExceptionDispatchInfo.Capture(operation.Exception).Throw();
            
            Assert.IsFalse(sender.Items.Contains(target));
            Assert.IsTrue(receiver.Items.Contains(target));

            Assert.IsTrue(operation.IsCompletedSuccessfully);
        }
        
        private static void AssertFail(
            IOperation operation, 
            ContainerItems sender,
            ContainerItems receiver,
            IUnit target)
        {
            Assert.IsTrue(operation.IsCompleted);
            
            Assert.IsTrue(sender.Items.Contains(target));
            Assert.IsFalse(receiver.Items.Contains(target));
            
            Assert.IsFalse(operation.IsCompletedSuccessfully);
        }
    }
}