using System;
using OperationSystem.Handlers;

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
    }
}