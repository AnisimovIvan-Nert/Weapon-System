using System;
using System.Collections;
using OperationSystem.Containers.Components;
using OperationSystem.Containers.Components.Containers;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Units;

namespace OperationSystem.Containers.Middleware
{
    public class ContainerVolumeMiddleware : AbstractOperationMiddleware<IReceiveOperationTag>
    {
        public override IEnumerator Validate(IOperation operation, IOperationContext context)
        {
            var unit = operation.GetData<IOperationUnit>().Unit;
            var world = context.World;
            
            if (!unit.TryGetComponent<IContainerVolume>(world, out var component))
                yield break;
            
            yield return CanReceive(component, operation, world);
        }

        public override IEnumerator TryAcquireLocks(IOperation operation, IOperationContext context)
        {
            var unit = operation.GetData<IOperationUnit>().Unit;
            var world = context.World;

            var componentsArray = world.TryGetComponentArray<IContainerVolume>(unit);
            if (componentsArray == null)
                yield break;

            context.Acquire(componentsArray.TypeId, unit.Id, operation.Identifier);
        }

        public override IEnumerator Execute(IOperation operation, IOperationContext context)
        {
            var unit = operation.GetData<IOperationUnit>().Unit;
            var world = context.World;
            
            if (!unit.TryGetComponent<IContainerVolume>(world, out var component))
                yield break;
            
            yield return CanReceive(component, operation, world);
        }

        private static IEnumerator CanReceive(IContainerVolume volume, IOperation operation, UnitWorld world)
        {
            var target = operation.GetData<IOperationTarget>().Target;
            
            if (!target.TryGetComponent<ISize>(world, out var size))
                yield break;

            var targetVolume = size.Height * size.Width;
            if (targetVolume > volume.MaxIndividualItemVolume)
                throw new InvalidOperationException();
        }
    }
}