using System;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.TaskBasedImplementation
{
    internal interface IInteraction
    {
        Guid Id { get; }
        bool IsCompleted { get; }
        ValueTask ExecuteAsync();
        void Cancel();
    }
}