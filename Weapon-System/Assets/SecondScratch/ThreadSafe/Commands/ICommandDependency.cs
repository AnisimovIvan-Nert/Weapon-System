using System;

namespace SecondScratch.ThreadSafe.Commands
{
    public interface ICommandDependency
    {
        bool IsCompleted { get; }

        event Action OnCompleted;
    }
}