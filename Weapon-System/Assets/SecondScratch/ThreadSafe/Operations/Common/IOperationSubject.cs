using System.Threading;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public interface IOperationSubject
    {
        ValueTask UpstreamOperation<T>(IOperation operation,Transaction transaction, T context, CancellationToken ct)
            where T : IOperationContext;
        
        ValueTask DownstreamOperation<T>(IOperation operation, Transaction transaction, T context, CancellationToken ct)
            where T : IOperationContext;
    }
}