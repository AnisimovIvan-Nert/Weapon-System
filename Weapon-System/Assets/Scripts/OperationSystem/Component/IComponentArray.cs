using ECS.Units;
using OperationSystem.Units;

namespace ECS
{
    public interface IComponentArray
    {
        void PullFromAssets(UnitRegistry registry);
        void PushToAssets(UnitRegistry registry);
        void OnEntityDestroyed(UnitId unitId);

        void SetAssetDirty(UnitId unitId);
    }
}
