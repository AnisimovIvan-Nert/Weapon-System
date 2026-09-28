using System;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations
{
    internal interface IOperation
    {
        Guid Id { get; }
        bool IsCompleted { get; }
        ValueTask Execute();
        void Cancel();
    }
}