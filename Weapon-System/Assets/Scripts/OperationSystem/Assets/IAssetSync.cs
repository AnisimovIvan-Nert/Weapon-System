using OperationSystem.Component;

namespace ECS
{
    public interface IAssetSync<T> : IAssetPull<T>, IAssetPush<T>
        where T : IComponent
    {
    }
}