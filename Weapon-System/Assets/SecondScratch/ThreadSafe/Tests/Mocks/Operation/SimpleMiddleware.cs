using System;
using System.Threading;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Operation
{
    public class SimpleMiddleware<TOperation, TContext> : IOperationMiddleware
    {
        public Action<TOperation, Transaction, TContext>? BeforeExecutionAction;
        public Action<TOperation, Transaction, TContext>? AfterExecutionAction;
        public Func<TOperation, Transaction, TContext, bool>? TryHandleExceptionAction;
        
        public ValueTask<bool> BeforeExecution<T>(IOperation operation, Transaction transaction, T context, CancellationToken ct) 
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || context is not TContext typedContext)
                return new ValueTask<bool>(false);
            
            BeforeExecutionAction?.Invoke(typedOperation, transaction, typedContext);
            
            return new ValueTask<bool>(true);
        }

        public ValueTask AfterExecution<T>(IOperation operation, Transaction transaction, T context, CancellationToken ct)
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || context is not TContext typedContext)
                return new ValueTask(Task.CompletedTask);
            
            AfterExecutionAction?.Invoke(typedOperation, transaction, typedContext);
            
            return new ValueTask(Task.CompletedTask);
        }

        public ValueTask<bool> TryHandleException<T>(IOperation operation, Exception exception, Transaction transaction, T context)
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || context is not TContext typedContext)
                return new ValueTask<bool>(false);
            
            var result = TryHandleExceptionAction?.Invoke(typedOperation, transaction, typedContext);
            result ??= false;
            
            return new ValueTask<bool>(result.Value);
        }
    }
}