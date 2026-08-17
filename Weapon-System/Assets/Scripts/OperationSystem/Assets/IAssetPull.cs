using OperationSystem.Component;
using OperationSystem.Units;

namespace OperationSystem.Assets
{
    public interface IAssetPull<T> 
        where T : IComponent
    {
        void PullInto(ref T component, UnitWorld world);
    }
}
