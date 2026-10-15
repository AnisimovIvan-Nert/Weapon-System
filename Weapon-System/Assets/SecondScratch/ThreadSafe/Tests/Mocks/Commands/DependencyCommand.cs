using System;
using SecondScratch.ThreadSafe.Commands;
using SecondScratch.ThreadSafe.LockableObject;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Commands
{
    public class DependencyCommand : AbstractCommand<DependencyObject>
    {
        public DependencyCommand(DependencyObject target, ICommandDependency? dependency = null) 
            : base(target, new CommandDependencyAggregator(target, dependency))
        {
        }

        protected override void InnerExecute()
        {
        }
    }

    public class DependencyObject : AbstractLockableObject, ICommandDependency
    {
        public bool IsCompleted { get; private set; }
        public event Action? OnCompleted;

        public void Complete()
        {
            IsCompleted = true;
            OnCompleted?.Invoke();
        }
    }
}