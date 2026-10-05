namespace SecondScratch.ThreadSafe.Commands
{
    public interface ICommand
    {
        object Target { get; }

        void Execute();
    }
}