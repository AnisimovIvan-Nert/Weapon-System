using System;
using System.Collections;
using Coroutine;
using OperationSystem.Containers.Units;
using OperationSystem.Containers.Units.Containers;
using OperationSystem.Containers.Units.Containers.Locks;
using OperationSystem.Containers.Units.Items;
using OperationSystem.Operations;
using OperationSystem.Operations.Units;
using OperationSystem.Units;

namespace OperationSystem.Containers.Operations
{
    public class ReceiveObjectOperation : AbstractStagedUnitOperation<IContainer>
    {
        private readonly IPlayer _executor;
        private readonly IUnit _target;
        
        public ReceiveObjectOperation(Guid identifier, IPlayer executor, IUnit target) 
            : base(identifier)
        {
            _executor = executor;
            _target = target;
        }

        protected override IEnumerator ValidateEnumerator()
        {
            var container = Handler.Unit ?? throw new InvalidOperationException();

            var containerLock = container.TryFind<IContainerLock>();
            if (containerLock != null)
            {
                bool? canInteract = null;
                yield return containerLock.CanInteractWithContainer(_executor)
                    .GetResult<bool>(result => canInteract = result);
                
                if (canInteract is not true) 
                    throw new InvalidOperationException();
            }

            var containerVolume = container.TryFind<IContainerVolume>();
            var targetSize = _target.TryFind<ISize>();
            if (containerVolume != null && targetSize != null)
            {
                var targetVolume = targetSize.Height * targetSize.Width;

                if (targetVolume > containerVolume.MaxIndividualItemVolume)
                    throw new InvalidOperationException();
            }

            var containerItems = container.Find<IContainerItems>();
            if (containerItems.Items.Contains(_target))
                throw new InvalidOperationException();
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            var container = Handler.Unit ?? throw new InvalidOperationException();
            
            var containerLock = container.TryFind<IContainerLock>();
            var containerItems = container.Find<IContainerItems>();

            if (containerLock != null)
                Context.Acquire(containerLock, this);
            
            Context.Acquire(containerItems, this);
            yield break;
        }

        protected override IEnumerator RecordPossibleMutationsEnumerator()
        {
            var items = Context.AccessFirst<IContainerItems>(this);

            Context.RecordUndo(() =>
            {
                if (items.Items.Contains(_target))
                    items.Items.Remove(_target);
            });
            
            yield break;
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            var items = Context.AccessFirst<IContainerItems>(this);
            var containerLock = Context.TryAccessFirst<IContainerLock>(this);
            
            if (containerLock != null)
            {
                bool? canInteract = null;
                yield return containerLock.CanInteractWithContainer(_executor)
                    .GetResult<bool>(result => canInteract = result);
                
                if (canInteract is not true) 
                    throw new InvalidOperationException();
            }

            items.Items.Add(_target);
        }
    }
}