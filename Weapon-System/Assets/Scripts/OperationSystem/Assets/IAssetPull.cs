using OperationSystem.Assets;
using OperationSystem.Component;

namespace ECS
{
    public interface IAssetPull<T> : IAsset 
        where T : IComponent
    {
        void PullInto(ref T component);
    }
}
