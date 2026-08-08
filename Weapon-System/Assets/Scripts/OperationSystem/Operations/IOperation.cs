using System;
using System.Threading.Tasks;
using OperationSystem.Handlers;
using OperationSystem.Operations.Data;

namespace OperationSystem.Operations
{
    public interface IOperation
    {
        OperationIdentifier Identifier { get; }
        
        bool IsCompleted { get; }
        bool IsCompletedSuccessfully { get; }
        public Exception? Exception { get; }
        
        void RunOperation(IOperationHandler handler, OperationStaging staging);
        
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