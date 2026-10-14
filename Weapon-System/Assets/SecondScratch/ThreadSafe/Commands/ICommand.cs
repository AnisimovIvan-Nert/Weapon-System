using System;

namespace SecondScratch.ThreadSafe.Commands
{
    public interface ICommand : ICommandDependency
    {
        object Target { get; }

        event EventHandler<CommandEventArg> OnReadyForExecute;

        void Execute();
        bool CanExecute();
    }

    public readonly struct CommandEventArg
    {
        public readonly ICommand Command;

        public CommandEventArg(ICommand command)
        {
            Command = command;
        }
    }
}