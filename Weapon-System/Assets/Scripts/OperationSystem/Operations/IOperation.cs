using System;
using System.Threading.Tasks;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Result;
using OperationSystem.Units;

namespace OperationSystem.Operations
{
    public interface IOperation
    {
        OperationIdentifier Identifier { get; }
        
        bool IsCompleted { get; }
        bool IsCompletedSuccessfully { get; }
        public Exception? Exception { get; }
        public IOperationResult? OperationResult { get; }
        
        void RunOperation(UnitWorld world, OperationStaging staging);
        
        void Increment(IOperationContext operationContext);
        Task RunStage(OperationStage stage);

        T? TryGetData<T>() where T : IOperationData;
    }

    public enum OperationStaging
    {
        None,
        Manual,
        Auto
    }

    public enum OperationStage
    {
        None,
        
        Initialization,
        
        Validate,
        
        TryAcquireLocks,
        ReleaseLocks,
        
        RecordMutations,
        Execute,
        
        Complete,
        Cancel
    }
}