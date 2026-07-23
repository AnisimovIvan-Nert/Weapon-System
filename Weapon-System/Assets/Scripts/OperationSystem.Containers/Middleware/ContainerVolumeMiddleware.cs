using System;
using System.Collections;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Containers.Units;
using OperationSystem.Containers.Units.Containers;
using OperationSystem.Handlers.Units;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Units;

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
            var volume = container.TryFind<IContainerVolume>();
            if (volume != null)
                yield return CanReceive(volume, operation);
        }

        public override IEnumerator TryAcquireLocks(
            IOperation operation, 
            IOperationContext context,
            IUnitOperationHandler handler)
        {
            var container = handler.Unit ?? throw new InvalidOperationException();
            var volume = container.TryFind<IContainerVolume>();
            
            if (volume == null)
                yield break;
            
            context.Acquire(volume, operation);
        }

        public override IEnumerator Execute(
            IOperation operation, 
            IOperationContext context, 
            IUnitOperationHandler handler)
        {
            var volume = context.TryAccessFirst<IContainerVolume>(operation);
            if (volume != null)
                yield return CanReceive(volume, operation);
        }

        private static IEnumerator CanReceive(IContainerVolume volume, IOperation operation)
        {
            var target = operation.GetData<IOperationTarget>();
            var size = target.Target.TryFind<ISize>();
            
            if (size == null)
                yield break;

            var targetVolume = size.Height * size.Width;
            if (targetVolume > volume.MaxIndividualItemVolume)
                throw new InvalidOperationException();
        }
    }
}