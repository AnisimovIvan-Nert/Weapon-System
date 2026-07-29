using OperationSystem.Operations;

namespace OperationSystem.Resource
{
    public interface IResource
    {
        bool IsLocked { get; }
        
        bool IsBelongs(OperationIdentifier owner);
        bool TryAcquire(OperationIdentifier owner);
        void Release(OperationIdentifier owner);
    }
}