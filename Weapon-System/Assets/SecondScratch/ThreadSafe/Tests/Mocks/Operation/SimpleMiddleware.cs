using System;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Operation
{
    public class SimpleMiddleware<TOperation, TContext> : IOperationMiddleware
        where TContext : IOperationContext
    {
        public Action<TOperation, OperationData<TContext>>? BeforeExecutionAction;
        public Action<TOperation, OperationData<TContext>>? AfterExecutionAction;
        public Func<TOperation, OperationData<TContext>, bool>? TryHandleExceptionAction;
        
        public ValueTask<bool> BeforeExecution<T>(IOperation operation, OperationData<T> data) 
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || data.Context is not TContext typedContext)
                return new ValueTask<bool>(false);

            var typedData = data.WithContext(typedContext);
            BeforeExecutionAction?.Invoke(typedOperation, typedData);
            
            return new ValueTask<bool>(true);
        }

        public ValueTask AfterExecution<T>(IOperation operation, OperationData<T> data)
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || data.Context is not TContext typedContext)
                return new ValueTask(Task.CompletedTask);
            
            var typedData = data.WithContext(typedContext);
            AfterExecutionAction?.Invoke(typedOperation, typedData);
            
            return new ValueTask(Task.CompletedTask);
        }

        public ValueTask<bool> TryHandleException<T>(IOperation operation, Exception exception, OperationData<T> data)
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || data.Context is not TContext typedContext)
                return new ValueTask<bool>(false);
            
            var typedData = data.WithContext(typedContext);
            var result = TryHandleExceptionAction?.Invoke(typedOperation, typedData);
            result ??= false;
            
            return new ValueTask<bool>(result.Value);
        }
    }
}