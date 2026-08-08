using System;
using System.Collections;
using OperationSystem.Containers.Components;
using OperationSystem.Containers.Components.Containers;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Handlers;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;

namespace OperationSystem.Containers.Middleware
{
    public class ContainerVolumeMiddleware : AbstractOperationMiddleware<IReceiveOperationTag>
    {
        public override IEnumerator Validate(
            IOperation operation,
            IOperationContext context,
            IOperationHandler handler)
        {
            var unit = handler.OperationUnit ?? throw new InvalidOperationException();
            var world = unit.World;

            var componentsArray = world.TryGetComponents<IContainerVolume>(unit);
            if (componentsArray == null)
                yield break;

            var component = componentsArray.GetComponent<IContainerVolume>(unit.Id);
            yield return CanReceive(component, operation);
        }

        public override IEnumerator TryAcquireLocks(
            IOperation operation,
            IOperationContext context,
            IOperationHandler handler)
        {
            var unit = handler.OperationUnit ?? throw new InvalidOperationException();
            var world = unit.World;

            var componentsArray = world.TryGetComponents<IContainerVolume>(unit);
            if (componentsArray == null)
                yield break;

            context.Acquire(componentsArray.TypeId, unit.Id, operation.Identifier);
        }

        public override IEnumerator Execute(
            IOperation operation,
            IOperationContext context,
            IOperationHandler handler)
        {
            var unit = handler.OperationUnit ?? throw new InvalidOperationException();
            var world = unit.World;

            var componentsArray = world.TryGetComponents<IContainerVolume>(unit);
            if (componentsArray == null)
                yield break;

            var component = componentsArray.GetComponent<IContainerVolume>(unit.Id);
            yield return CanReceive(component, operation);
        }

        private static IEnumerator CanReceive(IContainerVolume volume, IOperation operation)
        {
            var target = operation.GetData<IOperationTarget>().Target;
            var world = target.World;
            
            var componentsArray = world.TryGetComponents<ISize>(target);
            if (componentsArray == null)
                yield break;
            
            var size = componentsArray.GetComponent<ISize>(target.Id);

            var targetVolume = size.Height * size.Width;
            if (targetVolume > volume.MaxIndividualItemVolume)
                throw new InvalidOperationException();
        }
    }
}