using System;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public interface IOperationMiddleware
    {
        ValueTask<bool> BeforeExecution<T>(IOperation operation, OperationData<T> data)
            where T : IOperationContext;
        
        ValueTask AfterExecution<T>(IOperation operation, OperationData<T> data)
            where T : IOperationContext;
        
        ValueTask<bool> TryHandleException<T>(IOperation operation, Exception exception, OperationData<T> data)
            where T : IOperationContext;
    }
}