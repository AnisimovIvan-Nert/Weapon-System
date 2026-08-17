using System;
using System.Collections;
using System.Linq;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Units;
using OperationSystem.Units.Child;

namespace OperationSystem.Containers.Operations
{
    public class SendObjectUnitOperation : AbstractOperation, ISendOperationTag
    {
        private Unit Unit => this.GetData<IOperationUnit>().Unit;
        
        public SendObjectUnitOperation(
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

            var childrenComponent = Unit.GetComponent<ChildrenComponent>(Context.World);
            if (!childrenComponent.Children.Contains(target.Target))
                throw new InvalidOperationException();
        }

        protected override IEnumerator RecordMutationsEnumerator()
        {
            yield return base.RecordMutationsEnumerator();
            
            var operationTarget = this.GetData<IOperationTarget>();
            var target = operationTarget.Target;
            
            Context.RecordUndo(() =>
            {
                Unit.TryAddChild(target);
            });
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var target = this.GetData<IOperationTarget>();
            
            if (!Unit.TryRemoveChild(target.Target))
                throw new InvalidOperationException();
        }
    }
}