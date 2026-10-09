using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public interface IOperationMiddleware
    {
        ValueTask<bool> BeforeExecution<T>(
            IOperation operation,
            Transaction transaction,
            T context,
            CancellationToken ct)
            where T : IOperationContext;
        
        ValueTask AfterExecution<T>(
            IOperation operation, 
            Transaction transaction, 
            T context, 
            CancellationToken ct)
            where T : IOperationContext;
        
        ValueTask<bool> TryHandleException<T>(
            IOperation operation,
            Exception exception,
            Transaction transaction, 
            T context)
            where T : IOperationContext;
    }
}