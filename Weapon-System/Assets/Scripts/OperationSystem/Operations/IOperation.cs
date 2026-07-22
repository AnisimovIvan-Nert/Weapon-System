using System;
using OperationSystem.UnitHandlers;
using OperationSystem.Units;

namespace OperationSystem.Operations
{
    public interface IOperation
    {
        Guid Identifier { get; }
        
        bool IsCompleted { get; }
        bool IsCompletedSuccessfully { get; }
        public Exception? Exception { get; }
        
        void Increment(IOperationContext operationContext);
    }

    public interface IOperation<T> : IOperation
        where T : IUnit
    {
        void RunOperation(IUnitHandler<T> handler);
    }
}