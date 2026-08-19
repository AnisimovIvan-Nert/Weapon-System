using OperationSystem.Assets;

namespace OperationSystem.Operations.Data
{
    public interface IOperationExecutor : IOperationData
    {
        IAsset Executor { get; }
    }
    
    public readonly struct OperationExecutor : IOperationExecutor
    {
        public IAsset Executor { get; }
        
        public OperationExecutor(IAsset executor)
        {
            Executor = executor;
        }
    }
}