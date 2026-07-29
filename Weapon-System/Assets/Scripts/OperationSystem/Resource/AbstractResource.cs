using OperationSystem.Operations;

namespace OperationSystem.Resource
{
    public abstract class AbstractResource : IResource
    {
        private OperationIdentifier? _owner;

        public bool IsLocked => _owner != null;

        public bool IsBelongs(OperationIdentifier owner)
        {
            return _owner == owner;
        }
        
        public bool TryAcquire(OperationIdentifier owner)
        {
            if (_owner == owner)
                return true;
            
            if (IsLocked) 
                return false;
            
            _owner = owner;
            return true;
        }

        public void Release(OperationIdentifier owner)
        {
            if (_owner == owner)
                _owner = null;
        }

        public void ForceRelease()
        {
            _owner = null;
        }
    }
}