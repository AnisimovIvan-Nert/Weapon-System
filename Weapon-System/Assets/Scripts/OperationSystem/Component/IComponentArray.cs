using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Component
{
    public interface IComponentArray
    {
        void PullFromAssets(UnitId unitId, UnitRegistry unitRegistry);
        void PushToAssets(UnitId unitId, UnitRegistry unitRegistry);
        void OnEntityDestroyed(UnitId unitId);

        void SetAssetDirty(UnitId unitId);

        ref OperationIdentifier GetOwner(UnitId unitId);
    }
}
