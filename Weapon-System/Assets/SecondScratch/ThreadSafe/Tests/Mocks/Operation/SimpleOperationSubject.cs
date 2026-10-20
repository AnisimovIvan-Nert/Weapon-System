using System;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Operation
{
    public class SimpleOperationSubject<TOperation, TContext> : AbstractOperationSubject
        where TContext : IOperationContext
    {
        public Action<TOperation, TContext, OperationData>? UpstreamOperationAction;
        public Action<TOperation, TContext, OperationData>? DownstreamOperationAction;
        
        public override async ValueTask UpstreamOperation<T>(IOperation operation, T context, OperationData data)
        {
            await base.UpstreamOperation(operation, context, data);

            if (operation is not TOperation typedOperation || context is not TContext typedContext)
                return;
            
            UpstreamOperationAction?.Invoke(typedOperation, typedContext, data);
        }

        public override async ValueTask DownstreamOperation<T>(IOperation operation, T context, OperationData data) 
        {
            await base.DownstreamOperation(operation, context, data);
            
            if (operation is not TOperation typedOperation || context is not TContext typedContext)
                return;

            DownstreamOperationAction?.Invoke(typedOperation, typedContext, data);
        }
    }
}