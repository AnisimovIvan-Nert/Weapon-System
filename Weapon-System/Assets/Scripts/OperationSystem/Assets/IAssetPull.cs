using OperationSystem.Assets;

namespace ECS
{
    public interface IAssetPull<T> : IAsset 
        where T : IComponent
    {
        void PullInto(ref T component);
    }
}
