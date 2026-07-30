using OperationSystem.Component;

namespace OperationSystem.Assets
{
    public interface IAssetSync<T> : IAssetPull<T>, IAssetPush<T>
        where T : IComponent
    {
    }
}