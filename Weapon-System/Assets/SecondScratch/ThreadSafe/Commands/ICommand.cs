using SecondScratch.Shared;
using SecondScratch.ThreadSafe.LockableObject;

namespace SecondScratch.ThreadSafe.Commands
{
    public interface ICommand : ICommandDependency
    {
        ILockableObject Target { get; }

        ConditionalEvent? OnReadyForExecute { get; }

        void Execute();
        bool CanExecute();
    }
}