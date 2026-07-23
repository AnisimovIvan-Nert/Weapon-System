using OperationSystem.Units;

namespace OperationSystem.Operations.Data
{
    public interface IOperationExecutor : IOperationData
    {
        IUnit Executor { get; }
    }
    
    public class OperationExecutor : IOperationExecutor
    {
        public IUnit Executor { get; }
        
        public OperationExecutor(IUnit executor)
        {
            Executor = executor;
        }
    }
}