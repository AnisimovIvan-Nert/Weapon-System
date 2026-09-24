using System;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Commands
{
    public interface ICommand
    {
        object Target { get; }
        bool IsCompleted { get; }
        Exception? Exception { get; }

        void Execute();
        ValueTask WaitExecution();
    }
}