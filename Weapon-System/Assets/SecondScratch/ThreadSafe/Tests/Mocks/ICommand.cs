using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Tests.Mocks
{
    public interface ICommand
    {
        object Target { get; }
        Task ExecutionTask { get; }

        void Execute();
    }
}