using SecondScratch.Shared;

namespace SecondScratch.ThreadSafe.Commands
{
    public interface ICommandDependency
    {
        bool IsCompleted { get; }
        ConditionalEvent OnCompleted { get; }
    }
}