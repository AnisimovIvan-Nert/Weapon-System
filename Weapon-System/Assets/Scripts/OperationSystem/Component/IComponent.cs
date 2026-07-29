using OperationSystem.Resource;

namespace OperationSystem.Component
{
    public interface IComponent : IResource
    {
        bool IsDirty { get; }
        
        void SetDirty();
        void ResetDirty();
    }
}