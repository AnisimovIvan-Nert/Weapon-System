using SecondScratch.Shared;
using SecondScratch.ThreadSafe.Commands;
using SecondScratch.ThreadSafe.LockableObject;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Commands
{
    public class DependencyCommand : AbstractCommand<DependencyObject>
    {
        public DependencyCommand(DependencyObject target, ICommandDependency? dependency = null) 
            : base(target, CommandDependencyAggregator.Create(target, dependency))
        {
        }

        protected override void InnerExecute()
        {
        }
    }

    public class DependencyObject : AbstractLockableObject, ICommandDependency
    {
        public bool IsCompleted { get; private set; }
        public ConditionalEvent OnCompleted { get; } = new(() => false, false);

        public void Complete()
        {
            IsCompleted = true;
            OnCompleted.Invoke();
        }
    }
}