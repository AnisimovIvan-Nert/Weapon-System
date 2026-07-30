using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Component
{
    public interface IComponentArray
    {
        int TypeId { get; }
        
        bool HasComponent(Unit unit);
        
        void PullFromAssets(UnitId unitId, UnitRegistry unitRegistry);
        void PushToAssets(UnitId unitId, UnitRegistry unitRegistry);
        void OnEntityDestroyed(UnitId unitId);

        void SetAssetDirty(UnitId unitId);

        OperationIdentifier GetOwner(UnitId unitId);
        void SetOwner(UnitId unitId, OperationIdentifier owner);

        T GetComponent<T>(UnitId unitId) where T : IComponent;
        void SetComponent<T>(UnitId unitId, T component) where T : IComponent;
    }
    
    public interface IComponentArray<T> : IComponentArray
        where T : IComponent
    {
        T GetComponent(UnitId unitId);
        void SetComponent(UnitId unitId, T component);
    }
}
