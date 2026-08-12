using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Component
{
    public interface IComponentArray
    {
        int TypeId { get; }
        
        bool HasComponent(Unit unit);
        
        void PullFromAsset(Unit unit);
        void PushToAsset(Unit unit);
        
        bool TryAcquireComponent(UnitId unitId, OperationIdentifier owner);
        void ReleaseComponent(UnitId unitId, OperationIdentifier owner);

        T GetComponent<T>(UnitId unitId) where T : IComponent;
        void SetComponent<T>(UnitId unitId, T component) where T : IComponent;
        void DestroyComponent(UnitId unitId);
    }
    
    public interface IComponentArray<T> : IComponentArray
        where T : IComponent
    {
        T GetComponent(UnitId unitId);
        void SetComponent(UnitId unitId, T component);
    }
}
