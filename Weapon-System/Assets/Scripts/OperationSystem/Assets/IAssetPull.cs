using OperationSystem.Component;

namespace OperationSystem.Assets
{
    public interface IAssetPull<T> : IAsset 
        where T : IComponent
    {
        void PullInto(ref T component);
    }
}
