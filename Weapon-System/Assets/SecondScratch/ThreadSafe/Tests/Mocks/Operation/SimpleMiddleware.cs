using System;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Operation
{
    public class SimpleMiddleware<TOperation, TContext> : IOperationMiddleware
        where TContext : IOperationContext
    {
        public Action<TOperation, TContext, OperationData>? BeforeExecutionAction;
        public Action<TOperation, TContext, OperationData>? AfterExecutionAction;
        public Func<TOperation, TContext, OperationData, bool>? TryHandleExceptionAction;
        
        public ValueTask<bool> BeforeExecution<T>(IOperation operation, T context, OperationData data) 
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || context is not TContext typedContext)
                return new ValueTask<bool>(false);
            
            BeforeExecutionAction?.Invoke(typedOperation, typedContext, data);
            return new ValueTask<bool>(true);
        }

        public ValueTask AfterExecution<T>(IOperation operation, T context, OperationData data)
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || context is not TContext typedContext)
                return new ValueTask(Task.CompletedTask);

            AfterExecutionAction?.Invoke(typedOperation, typedContext, data);
            return new ValueTask(Task.CompletedTask);
        }

        public ValueTask<bool> TryHandleException<T>(
            IOperation operation,
            Exception exception,
            T context,
            OperationData data)
            where T : IOperationContext
        {
            if (operation is not TOperation typedOperation || context is not TContext typedContext)
                return new ValueTask<bool>(false);

            var result = TryHandleExceptionAction?.Invoke(typedOperation, typedContext, data) ?? false;
            return new ValueTask<bool>(result);
        }
    }
}