using OperationSystem.Resource;

namespace OperationSystem.Component
{
    public abstract class AbstractComponent : AbstractResource, IComponent
    {
        public bool IsDirty { get; protected set; }
        
        public virtual void SetDirty() => IsDirty = true;
        public virtual void ResetDirty() => IsDirty = false;
    }
}