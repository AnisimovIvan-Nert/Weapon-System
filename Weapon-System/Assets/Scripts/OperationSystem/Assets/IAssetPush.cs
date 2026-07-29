using OperationSystem.Assets;

namespace ECS
{
    public interface IAssetPush<T> : IAsset
        where T : IComponent
    {
        void PushFrom(in T component);
    }
}
