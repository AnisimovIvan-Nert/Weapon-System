namespace SecondScratch.ThreadSafe.Commands
{
    public interface ICommand
    {
        object Target { get; }
        bool IsCompleted { get; }

        void Execute();
        bool CanExecute();
    }
}