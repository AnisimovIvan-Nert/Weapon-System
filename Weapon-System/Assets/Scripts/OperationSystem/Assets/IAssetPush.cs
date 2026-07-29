using OperationSystem.Assets;
using OperationSystem.Component;

namespace ECS
{
    public interface IAssetPush<T> : IAsset
        where T : IComponent
    {
        void PushFrom(in T component);
    }
}
