namespace Weapons.Resource
{
    public abstract class AbstractResource : IResource
    {
        private object? _owner;

        public bool IsLocked => _owner != null;

        public bool IsBelongs(object owner)
        {
            return _owner == owner;
        }
        
        public bool TryAcquire(object owner)
        {
            if (_owner == owner)
                return true;
            
            if (IsLocked) 
                return false;
            
            _owner = owner;
            return true;
        }

        public void Release(object owner)
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