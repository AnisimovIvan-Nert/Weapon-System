using System;
using System.Collections;
using Coroutine;
using OperationSystem.Containers.Components.Containers.Locks;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Handlers;
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
            IOperationHandler handler)
        {
            var unit = operation.GetData<IOperationUnit>().Unit;
            var world = unit.World;
            
            var componentsArray = world.TryGetComponents<IContainerLock>(unit);
            if (componentsArray == null)
                yield break;

            var component = componentsArray.GetComponent<IContainerLock>(unit.Id);
            yield return CanInteract(component, operation);
        }

        public override IEnumerator TryAcquireLocks(
            IOperation operation, 
            IOperationContext context,
            IOperationHandler handler)
        {
            var unit = operation.GetData<IOperationUnit>().Unit;
            var world = unit.World;
            
            var componentsArray = world.TryGetComponents<IContainerLock>(unit);
            if (componentsArray == null)
                yield break;
            
            context.Acquire(componentsArray.TypeId, unit.Id, operation.Identifier);
        }

        public override IEnumerator Execute(
            IOperation operation, 
            IOperationContext context, 
            IOperationHandler handler)
        {
            var unit = operation.GetData<IOperationUnit>().Unit;
            var world = unit.World;
            
            var componentsArray = world.TryGetComponents<IContainerLock>(unit);
            if (componentsArray == null)
                yield break;

            var component = componentsArray.GetComponent<IContainerLock>(unit.Id);
            yield return CanInteract(component, operation);
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