using System;
using System.Collections;
using System.Linq;
using OperationSystem.Containers.Tests.Mocks;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;

namespace OperationSystem.Containers.Operations
{
    public class SendObjectUnitOperation : AbstractOperation
    {
        private ContainerAsset Container => (ContainerAsset)this.GetData<IOperationAsset>().Asset;
        
        public SendObjectUnitOperation(
            OperationIdentifier identifier,
            IOperationAsset operationAsset,
            IOperationExecutor executor, 
            IOperationTarget target) 
            : base(identifier, operationAsset, executor, target)
        {
        }

        protected override IEnumerator ValidateEnumerator()
        {
            yield return base.ValidateEnumerator();
            
            var target = this.GetData<IOperationTarget>();
            
            if (!Container.Children.Contains(target.Target))
                throw new InvalidOperationException();
        }

        protected override IEnumerator RecordMutationsEnumerator()
        {
            yield return base.RecordMutationsEnumerator();
            
            var operationTarget = this.GetData<IOperationTarget>();
            var target = operationTarget.Target;
            var container = Container;
            
            Context.RecordUndo(() =>
            {
                container.TryAddChild(target);
            });
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var target = this.GetData<IOperationTarget>();
            
            if (!Container.TryRemoveChild(target.Target))
                throw new InvalidOperationException();
        }
    }
}