using OperationSystem.Assets;

namespace OperationSystem.Operations.Data
{
    public interface IOperationAsset : IOperationData
    {
        IAsset Asset { get; }
    }
    
    public struct OperationAsset : IOperationAsset
    {
        public IAsset Asset { get; }
        
        public OperationAsset(IAsset asset)
        {
            Asset = asset;
        }
    }
}