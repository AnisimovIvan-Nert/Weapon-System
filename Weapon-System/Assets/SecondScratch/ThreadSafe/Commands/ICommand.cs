using System;

namespace SecondScratch.ThreadSafe.Commands
{
    public interface ICommand : ICommandDependency
    {
        object Target { get; }

        event Action OnReadyForExecute;

        void Execute();
        bool CanExecute();
    }
}