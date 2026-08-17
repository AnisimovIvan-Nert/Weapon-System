using OperationSystem.Component;
using OperationSystem.Units;

namespace OperationSystem.Assets
{
    public interface IAssetPush<T>
        where T : IComponent
    {
        void PushFrom(in T component, UnitWorld world);
    }
}
