using System;
using System.Collections;
using Coroutine;
using OperationSystem.Containers.Components.Containers.Locks;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Handlers.Units;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;

namespace OperationSystem.Containers.Middleware
{
    public class ContainerLockMiddleware : AbstractOperationMiddleware<IContainerOperationTag>
    {
        public override IEnumerator Validate(
            IOperation operation, 
            IOperationContext context,
            IUnitOperationHandler handler)
        {
            var container = handler.Unit ?? throw new InvalidOperationException();
            var containerComponents = container.ComponentsData;
            
            var containerLock = containerComponents.TryGet<IContainerLock>();
            if (containerLock != null)
                yield return CanInteract(containerLock, operation);
        }

        public override IEnumerator TryAcquireLocks(
            IOperation operation, 
            IOperationContext context,
            IUnitOperationHandler handler)
        {
            var container = handler.Unit ?? throw new InvalidOperationException();
            var containerLock = container.ComponentsData.TryGet<IContainerLock>();
            
            if (containerLock == null)
                yield break;
            
            context.Acquire(containerLock, operation.Identifier);
        }

        public override IEnumerator Execute(
            IOperation operation, 
            IOperationContext context, 
            IUnitOperationHandler handler)
        {
            var containerLock = context.TryAccess<IContainerLock>(operation.Identifier);
            if (containerLock != null)
                yield return CanInteract(containerLock, operation);
        }

        private static IEnumerator CanInteract(IContainerLock containerLock, IOperation operation)
        {
            var executor = operation.GetData<IOperationExecutor>();
            
            bool? canInteract = null;
            yield return containerLock.CanInteractWithContainer(executor.Executor)
                .GetResult<bool>(result => canInteract = result);
                
            if (canInteract is not true) 
                throw new InvalidOperationException();
        }
    }
}