using System;
using System.Collections;
using OperationSystem.Containers.Components.Containers;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Units;

namespace OperationSystem.Containers.Operations
{
    public class ReceiveObjectOperation : AbstractOperation, IReceiveOperationTag
    {
        private Unit Unit => this.GetData<IOperationUnit>().Unit;
        
        public ReceiveObjectOperation(
            OperationIdentifier identifier,
            IOperationUnit operationUnit,
            IOperationExecutor executor, 
            IOperationTarget target) 
            : base(identifier, operationUnit, executor, target)
        {
        }

        protected override IEnumerator ValidateEnumerator()
        {
            yield return base.ValidateEnumerator();
            
            var target = this.GetData<IOperationTarget>();

            var containerItems = Unit.GetComponent<ContainerItems>(Context.World);
            if (containerItems.Items.Contains(target.Target))
                throw new InvalidOperationException();
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();
            
            Context.Acquire<ContainerItems>(Unit, Identifier);
        }

        protected override IEnumerator RecordMutationsEnumerator()
        {
            yield return base.RecordMutationsEnumerator();
            
            var operationTarget = this.GetData<IOperationTarget>();
            var target = operationTarget.Target;
            var containerItems = Unit.GetComponent<ContainerItems>(Context.World);

            Context.RecordUndo(() =>
            {
                if (containerItems.Items.Contains(target))
                    containerItems.Items.Remove(target);
            });
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var target = this.GetData<IOperationTarget>();
            
            var containerItems = Unit.GetComponent<ContainerItems>(Context.World);
            containerItems.Items.Add(target.Target);
            Unit.SetComponent(containerItems, Context.World);
        }
    }
}