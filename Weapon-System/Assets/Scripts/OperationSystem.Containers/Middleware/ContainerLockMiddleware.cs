using System;
using System.Collections;
using Coroutine;
using OperationSystem.Containers.Components.Containers.Locks;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Units;

namespace OperationSystem.Containers.Middleware
{
    public class ContainerLockMiddleware : AbstractOperationMiddleware<IContainerOperationTag>
    {
        public override IEnumerator Validate(IOperation operation, IOperationContext context)
        {
            var unit = operation.GetData<IOperationUnit>().Unit;
            var world = context.World;
            
            if (!unit.TryGetComponent<IContainerLock>(world, out var component))
                yield break;
            
            yield return CanInteract(component, operation, world);
        }

        public override IEnumerator TryAcquireLocks(IOperation operation, IOperationContext context)
        {
            var unit = operation.GetData<IOperationUnit>().Unit;
            var world = context.World;
            
            var componentsArray = world.TryGetComponentArray<IContainerLock>(unit);
            if (componentsArray == null)
                yield break;
            
            context.Acquire(componentsArray.TypeId, unit, operation.Identifier);
        }

        public override IEnumerator Execute(IOperation operation, IOperationContext context)
        {
            var unit = operation.GetData<IOperationUnit>().Unit;
            var world = context.World;
            
            if (!unit.TryGetComponent<IContainerLock>(world, out var component))
                yield break;
            
            yield return CanInteract(component, operation, world);
        }

        private static IEnumerator CanInteract(IContainerLock containerLock, IOperation operation, UnitWorld world)
        {
            var executor = operation.GetData<IOperationExecutor>();
            
            bool? canInteract = null;
            yield return containerLock.CanInteractWithContainer(executor.Executor, world)
                .GetResult<bool>(result => canInteract = result);
                
            if (canInteract is not true) 
                throw new InvalidOperationException();
        }
        
    }
}