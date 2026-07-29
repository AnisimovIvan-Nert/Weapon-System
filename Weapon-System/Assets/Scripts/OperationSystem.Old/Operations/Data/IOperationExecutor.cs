using OperationSystem.Units;

namespace OperationSystem.Operations.Data
{
    public interface IOperationExecutor : IOperationData
    {
        Unit Executor { get; }
    }
    
    public class OperationExecutor : IOperationExecutor
    {
        public Unit Executor { get; }
        
        public OperationExecutor(Unit executor)
        {
            Executor = executor;
        }
    }
}