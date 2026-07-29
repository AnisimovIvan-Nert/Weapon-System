using OperationSystem.Units;

namespace ECS
{
    public interface IComponentArray
    {
        int ComponentTypeId { get; }
        void PullFromAssets(UnitRegistry registry, ISyncAssetResolver assetResolver);
        void PushToAssets(UnitRegistry registry, ISyncAssetResolver assetResolver);
        void OnEntityDestroyed(UnitId unitId);
        void Grow(int newCapacity);

        void SetAssetDirty(UnitId unitId);
    }
}
