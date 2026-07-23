using System;
using OperationSystem.Handlers;
using OperationSystem.Operations.Data;

namespace OperationSystem.Operations
{
    public interface IOperation
    {
        Guid Identifier { get; }
        
        bool IsCompleted { get; }
        bool IsCompletedSuccessfully { get; }
        public Exception? Exception { get; }
        
        void Increment(IOperationContext operationContext);
        
        void RunOperation(IOperationHandler handler);

        T? TryGetData<T>()
            where T : IOperationData;
    }
}