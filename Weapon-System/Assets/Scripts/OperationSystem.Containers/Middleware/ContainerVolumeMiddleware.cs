using System;
using System.Collections;
using OperationSystem.Containers.Components;
using OperationSystem.Containers.Components.Containers;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Handlers.Units;
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
            IUnitOperationHandler handler)
        {
            var container = handler.Unit ?? throw new InvalidOperationException();
            var volume = container.ComponentsData.TryGet<IContainerVolume>();
            if (volume != null)
                yield return CanReceive(volume, operation);
        }

        public override IEnumerator TryAcquireLocks(
            IOperation operation, 
            IOperationContext context,
            IUnitOperationHandler handler)
        {
            var container = handler.Unit ?? throw new InvalidOperationException();
            var volume = container.ComponentsData.TryGet<IContainerVolume>();
            
            if (volume == null)
                yield break;
            
            context.Acquire(volume, operation.Identifier);
        }

        public override IEnumerator Execute(
            IOperation operation, 
            IOperationContext context, 
            IUnitOperationHandler handler)
        {
            var volume = context.TryAccess<IContainerVolume>(operation.Identifier);
            if (volume != null)
                yield return CanReceive(volume, operation);
        }

        private static IEnumerator CanReceive(IContainerVolume volume, IOperation operation)
        {
            var target = operation.GetData<IOperationTarget>();
            var size = target.Target.ComponentsData.TryGet<ISize>();
            
            if (size == null)
                yield break;

            var targetVolume = size.Height * size.Width;
            if (targetVolume > volume.MaxIndividualItemVolume)
                throw new InvalidOperationException();
        }
    }
}