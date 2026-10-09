using System;
using System.Threading;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Operation
{
    public class SimpleOperationSubject<TOperation, TContext> : IOperationSubject
    {
        public Action<TOperation, Transaction, TContext>? UpstreamOperationAction;
        public Action<TOperation, Transaction, TContext>? DownstreamOperationAction;
        
        public ValueTask UpstreamOperation<T>(IOperation operation, Transaction transaction, T context, CancellationToken ct)
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || context is not TContext typedContext)
                return new ValueTask(Task.CompletedTask);
            
            UpstreamOperationAction?.Invoke(typedOperation, transaction, typedContext);
            
            return new ValueTask(Task.CompletedTask);
        }

        public ValueTask DownstreamOperation<T>(IOperation operation, Transaction transaction, T context, CancellationToken ct) 
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || context is not TContext typedContext)
                return new ValueTask(Task.CompletedTask);
            
            DownstreamOperationAction?.Invoke(typedOperation, transaction, typedContext);
            
            return new ValueTask(Task.CompletedTask);
        }
    }
}