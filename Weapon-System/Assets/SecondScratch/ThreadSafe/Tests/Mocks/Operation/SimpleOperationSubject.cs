using System;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Operation
{
    public class SimpleOperationSubject<TOperation, TContext> : AbstractOperationSubject
        where TContext : IOperationContext
    {
        public Action<TOperation, OperationData<TContext>>? UpstreamOperationAction;
        public Action<TOperation, OperationData<TContext>>? DownstreamOperationAction;
        
        public override async ValueTask UpstreamOperation<T>(IOperation operation, OperationData<T> data)
        {
            await base.UpstreamOperation(operation, data);

            if (operation is not TOperation typedOperation || data.Context is not TContext typedContext)
                return;

            var typedData = data.WithContext(typedContext);
            UpstreamOperationAction?.Invoke(typedOperation, typedData);
        }

        public override async ValueTask DownstreamOperation<T>(IOperation operation, OperationData<T> data) 
        {
            await base.DownstreamOperation(operation, data);
            
            if (operation is not TOperation typedOperation || data.Context is not TContext typedContext)
                return;
            
            var typedData = data.WithContext(typedContext);
            DownstreamOperationAction?.Invoke(typedOperation, typedData);
        }
    }
}