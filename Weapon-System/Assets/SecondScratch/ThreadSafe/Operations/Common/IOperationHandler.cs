using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public interface IOperationHandler : IOperation
    {
        ValueTask Execute();
        void Cancel();
    }
}