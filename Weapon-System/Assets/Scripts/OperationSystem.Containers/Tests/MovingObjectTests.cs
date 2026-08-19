using System.Linq;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using OperationSystem.Containers.Operations;
using OperationSystem.Containers.Tests.Mocks;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.TestExtensions;
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
            var operationRunner = new OperationRunner();
            
            var target = gameObject.AddComponent<FooAsset>();
            
            var sender = gameObject.AddComponent<ContainerAsset>();
            sender.Set(children: target);
            
            var receiver = gameObject.AddComponent<ContainerAsset>();
            
            var executor = gameObject.AddComponent<ExecutorAsset>();

            var operation = RunAndWaitOperation(executor, target, sender, receiver, operationRunner);
            
            AssertPass(operation, sender, receiver, target);
        }
        
        private static IOperation RunAndWaitOperation(
            ExecutorAsset executor,
            FooAsset target, 
            ContainerAsset sender, 
            ContainerAsset receiver,
            IOperationRunner operationRunner)
        {
            var executorData = new OperationExecutor(executor);
            var targetData = new OperationTarget(target);
            
            var operation = new MovingObjectOperation(OperationIdentifier.CreateNew(), executorData, targetData, 
                sender, receiver);
            
            operation.RunOperation(operationRunner);
            operationRunner.UpdateUntilComplete(operation, Timeout);

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