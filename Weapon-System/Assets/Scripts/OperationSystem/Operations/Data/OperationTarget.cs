using OperationSystem.Assets;

namespace OperationSystem.Operations.Data
{
    public interface IOperationTarget : IOperationData
    {
        IAsset Target { get; }
    }
    
    public readonly struct OperationTarget : IOperationTarget
    {
        public IAsset Target { get; }
        
        public OperationTarget(IAsset target)
        {
            Target = target;
        }
    }
}