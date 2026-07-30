using OperationSystem.Component;

namespace OperationSystem.Assets
{
    public interface IAssetPush<T> : IAsset
        where T : IComponent
    {
        void PushFrom(in T component);
    }
}
