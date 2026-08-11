using OperationSystem.Units;

namespace OperationSystem.Operations.Data
{
    public interface IOperationExecutor : IOperationData
    {
        Unit Executor { get; }
    }
    
    public readonly struct OperationExecutor : IOperationExecutor
    {
        public Unit Executor { get; }
        
        public OperationExecutor(Unit executor)
        {
            Executor = executor;
        }
    }
}