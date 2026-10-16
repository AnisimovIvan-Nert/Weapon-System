using SecondScratch.ThreadSafe.Commands;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Operations
{
    public abstract partial class AbstractOperationSubject
    {
        public AddChildCommand CreateAddChildCommand(
            IOperationSubject child,
            ICommandDependency? dependency = null) =>
            new(this, child, dependency);
        
        public RemoveChildCommand CreateRemoveChildCommand(
            IOperationSubject child,
            ICommandDependency? dependency = null) =>
            new(this, child, dependency);
        
        public class AddChildCommand : AbstractCommand<AbstractOperationSubject>
        {
            private readonly IOperationSubject _child;
            
            public AddChildCommand(
                AbstractOperationSubject target, 
                IOperationSubject child, 
                ICommandDependency? dependency = null) 
                : base(target, dependency)
            {
                _child = child;
            }

            protected override void InnerExecute() => TypedTarget.AddChild(_child);
        }
        
        public class RemoveChildCommand : AbstractCommand<AbstractOperationSubject>
        {
            private readonly IOperationSubject _child;
            
            public RemoveChildCommand(
                AbstractOperationSubject target, 
                IOperationSubject child, 
                ICommandDependency? dependency = null) 
                : base(target, dependency)
            {
                _child = child;
            }

            protected override void InnerExecute() => TypedTarget.RemoveChild(_child);
        }
    }
}