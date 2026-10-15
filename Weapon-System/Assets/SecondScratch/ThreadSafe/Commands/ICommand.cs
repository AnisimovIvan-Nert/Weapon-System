using SecondScratch.Shared;

namespace SecondScratch.ThreadSafe.Commands
{
    public interface ICommand : ICommandDependency
    {
        object Target { get; }

        ConditionalEvent? OnReadyForExecute { get; }

        void Execute();
        bool CanExecute();
    }
}