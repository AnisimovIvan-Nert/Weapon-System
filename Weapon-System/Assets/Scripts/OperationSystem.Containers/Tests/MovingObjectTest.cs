using System;
using Coroutine;
using NUnit.Framework;
using OperationSystem.Containers.Operations;
using OperationSystem.Containers.Tests.Mocks;
using OperationSystem.Containers.UnitHandlers;
using OperationSystem.Containers.Units;
using OperationSystem.Containers.Units.Containers;
using OperationSystem.Handlers;
using OperationSystem.Operations;

namespace OperationSystem.Containers.Tests
{
    public class MovingObjectTest
    {
        private const int Timeout = 100;
        
        [Test]
        public void Test()
        {
            var target = new FooUnit();

            var senderItems = new ContainerItems(target);
            var sender = new Container(senderItems);

            var receiverItems = new ContainerItems();
            var receiver = new Container(receiverItems);

            var executor = new Player();

            var senderRunner = new OperationRunner();
            var senderHandler = new ContainerHandler(senderRunner);
            senderHandler.SetUnit(sender).Wait();

            var receiverRunner = new OperationRunner();
            var receiverHandler = new ContainerHandler(receiverRunner);
            receiverHandler.SetUnit(receiver).Wait();

            var globalRunner = new OperationRunner();
            var globalHandler = new OperationHandler(globalRunner);

            var guid = Guid.NewGuid();
            var moveOperation = new MovingObjectOperation(guid, executor, target, senderHandler, receiverHandler);
            moveOperation.RunOperation(globalHandler);
            
            var timeout = Timeout;
            while (!moveOperation.IsCompleted && timeout > 0)
            {
                timeout--;
                globalHandler.Update();
                senderHandler.Update();
                receiverHandler.Update();
            }
            
            if (!moveOperation.IsCompleted)
                Assert.Fail();
            
            if (moveOperation.IsCompletedSuccessfully)
                Assert.Pass();
            
            Assert.Fail();
        }
    }
}