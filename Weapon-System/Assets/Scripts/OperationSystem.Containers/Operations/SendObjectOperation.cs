using System;
using System.Collections;
using Coroutine;
using OperationSystem.Containers.Units;
using OperationSystem.Containers.Units.Containers;
using OperationSystem.Containers.Units.Containers.Locks;
using OperationSystem.Operations;
using OperationSystem.Operations.Units;
using OperationSystem.Units;

namespace OperationSystem.Containers.Operations
{
    public class SendObjectUnitOperation : AbstractStagedUnitOperation<IContainer>
    {
        private readonly IPlayer _executor;
        private readonly IUnit _target;
        
        public SendObjectUnitOperation(Guid identifier, IPlayer executor, IUnit target) 
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

            var containerItems = container.Find<IContainerItems>();

            if (!containerItems.Items.Contains(_target))
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
            var index = items.Items.IndexOf(_target);

            if (index == -1)
                throw new InvalidOperationException();
            
            Context.RecordUndo(() =>
            {
                if (items.Items.Contains(_target))
                    items.Items.Remove(_target);
                
                items.Items.Insert(index, _target);
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

            items.Items.Remove(_target);
        }
    }
}