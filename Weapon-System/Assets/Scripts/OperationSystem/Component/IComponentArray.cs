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
        
        bool TryAcquireComponent(Unit unit, OperationIdentifier owner);
        void ReleaseComponent(Unit unit, OperationIdentifier owner);

        T GetComponent<T>(Unit unit) where T : IComponent;
        void SetComponent<T>(Unit unit, T component) where T : IComponent;
        void DestroyComponent(Unit unit);
    }
    
    public interface IComponentArray<T> : IComponentArray
        where T : IComponent
    {
        T GetComponent(Unit unit);
        void SetComponent(Unit unit, T component);
    }
}
