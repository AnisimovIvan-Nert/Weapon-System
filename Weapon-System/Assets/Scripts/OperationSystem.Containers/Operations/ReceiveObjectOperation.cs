using System;
using System.Collections;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Containers.Units.Containers;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Units;
using OperationSystem.Units;

namespace OperationSystem.Containers.Operations
{
    public class ReceiveObjectOperation : AbstractStagedUnitOperation<IContainer>, IReceiveOperationTag
    {
        public ReceiveObjectOperation(
            Guid identifier, 
            IOperationExecutor executor, 
            IOperationTarget target, 
            IOperationMiddleware[] middlewares) 
            : base(identifier, middlewares, executor, target)
        {
        }

        protected override IEnumerator ValidateEnumerator()
        {
            yield return base.ValidateEnumerator();
            
            var container = Handler.Unit ?? throw new InvalidOperationException();
            var target = this.GetData<IOperationTarget>();

            var containerItems = container.Find<IContainerItems>();
            if (containerItems.Items.Contains(target.Target))
                throw new InvalidOperationException();
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();
            
            var container = Handler.Unit ?? throw new InvalidOperationException();
            var containerItems = container.Find<IContainerItems>();
            Context.Acquire(containerItems, this);
        }

        protected override IEnumerator RecordPossibleMutationsEnumerator()
        {
            yield return base.RecordPossibleMutationsEnumerator();
            
            var operationTarget = this.GetData<IOperationTarget>();
            var target = operationTarget.Target;
            var items = Context.AccessFirst<IContainerItems>(this);

            Context.RecordUndo(() =>
            {
                if (items.Items.Contains(target))
                    items.Items.Remove(target);
            });
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var items = Context.AccessFirst<IContainerItems>(this);
            var target = this.GetData<IOperationTarget>();

            items.Items.Add(target.Target);
        }
    }
}