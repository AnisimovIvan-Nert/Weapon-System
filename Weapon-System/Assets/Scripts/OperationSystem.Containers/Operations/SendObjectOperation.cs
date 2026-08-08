using System;
using System.Collections;
using OperationSystem.Component;
using OperationSystem.Containers.Components.Containers;
using OperationSystem.Containers.Operations.Tags;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Units;

namespace OperationSystem.Containers.Operations
{
    public class SendObjectUnitOperation : AbstractOperation, ISendOperationTag
    {
        private Unit Unit => Handler.OperationUnit ?? throw new InvalidOperationException();
        private UnitWorld World => Unit.World;

        private ComponentArray<ContainerItems> ItemsComponents => World.GetComponents<ContainerItems>();
        
        public SendObjectUnitOperation(
            OperationIdentifier identifier, 
            IOperationExecutor executor, 
            IOperationTarget target, 
            IOperationMiddleware[] middlewares) 
            : base(identifier, middlewares, executor, target)
        {
        }

        protected override IEnumerator ValidateEnumerator()
        {
            yield return base.ValidateEnumerator();
            
            var target = this.GetData<IOperationTarget>();

            var containerItems = ItemsComponents.GetComponent(Unit.Id);
            if (!containerItems.Items.Contains(target.Target))
                throw new InvalidOperationException();
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();
            
            Context.Acquire<ContainerItems>(Unit.Id, Identifier);
        }

        protected override IEnumerator RecordMutationsEnumerator()
        {
            yield return base.RecordMutationsEnumerator();
            
            var operationTarget = this.GetData<IOperationTarget>();
            var target = operationTarget.Target;
            var containerItems = ItemsComponents.GetComponent(Unit.Id);
            var index = containerItems.Items.IndexOf(target);

            if (index == -1)
                throw new InvalidOperationException();
            
            Context.RecordUndo(() =>
            {
                if (containerItems.Items.Contains(target))
                    containerItems.Items.Remove(target);
                
                containerItems.Items.Insert(index, target);
            });
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var target = this.GetData<IOperationTarget>();
            
            var containerItems = ItemsComponents.GetComponent(Unit.Id);
            containerItems.Items.Remove(target.Target);
            ItemsComponents.SetComponent(Unit.Id, containerItems);
        }
    }
}