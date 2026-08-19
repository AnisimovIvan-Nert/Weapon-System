using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OperationSystem.Assets;
using OperationSystem.Containers.Operations;
using OperationSystem.Containers.Tests.Mocks;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.TestExtensions;
using UnityEngine;

namespace OperationSystem.Containers.Tests
{
    public class MultithreadMovingObjectTests
    {
        private const int Timeout = 1000;
        private const int UpdateDelay = 1;

        [Test]
        public async Task ExecutionTest()
        {
            const int executorCount = 100;

            var gameObject = new GameObject();

            var targets = new FooAsset[executorCount];
            var containerAssets = new (ContainerAsset, ContainerAsset)[executorCount];
            var operations = new IOperation[executorCount];
            var tasks = new Task[executorCount];
            for (var i = 0; i < executorCount; i++)
            {
                targets[i] = gameObject.AddComponent<FooAsset>();

                var containerItems = new IAsset[] { targets[i] };
                var emptyItems = Array.Empty<IAsset>();

                var firstItems = (i & 1) == 1 ? containerItems : emptyItems;
                var firstContainer = gameObject.AddComponent<ContainerAsset>();
                firstContainer.Set(children: firstItems);

                var secondItems = (i & 1) == 1 ? emptyItems : containerItems;
                var secondContainer = gameObject.AddComponent<ContainerAsset>();
                secondContainer.Set(children: secondItems);

                containerAssets[i] = (firstContainer, secondContainer);

                var executor = gameObject.AddComponent<ExecutorAsset>();

                var (senderAsset, receiverAsset) = (i & 1) == 1
                    ? (firstContainer, secondContainer)
                    : (secondContainer, firstContainer);

                (operations[i], tasks[i]) = RunOperation(executor, targets[i], senderAsset, receiverAsset);
            }

            await Task.WhenAll(tasks);

            for (var i = 0; i < executorCount; i++)
            {
                operations[i].AssertPass();

                var senderAsset = (i & 1) == 1 ? containerAssets[i].Item1 : containerAssets[i].Item2;
                var receiverAsset = (i & 1) == 1 ? containerAssets[i].Item2 : containerAssets[i].Item1;
                var target = targets[i];

                Assert.IsFalse(senderAsset.Children.Contains(target));
                Assert.IsTrue(receiverAsset.Children.Contains(target));
            }
        }

        private static (IOperation, Task) RunOperation(
            ExecutorAsset executor,
            FooAsset target,
            ContainerAsset sender,
            ContainerAsset receiver)
        {
            var executorData = new OperationExecutor(executor);
            var targetData = new OperationTarget(target);

            var operation = new MovingObjectOperation(OperationIdentifier.CreateNew(), executorData, targetData,
                sender, receiver);

            var task = operation.RunOperationAsTask(Timeout, UpdateDelay);
            return (operation, task);
        }
    }
}