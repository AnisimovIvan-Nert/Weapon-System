using System.Collections.Concurrent;
using System.Collections.Generic;
using OperationSystem.Operations;
using UnityEngine;

namespace OperationSystem.Assets
{
    public abstract class AbstractAsset 
        : MonoBehaviour
        , IAsset
    {
        private OperationIdentifier _owner;
        private object _ownerLock = new();
        
        private ConcurrentDictionary<IAsset, byte>? _childrenDictionary;
        private ConcurrentDictionary<IAsset, byte> ChildrenDictionary => _childrenDictionary ?? InitializeChildren();

        public virtual IEnumerable<IAsset> Children => ChildrenDictionary.Keys;
        
        public virtual bool TryAddChild(IAsset child) => ChildrenDictionary.TryAdd(child, 0);
        public virtual bool TryRemoveChild(IAsset child) => ChildrenDictionary.TryRemove(child, out _);

        private ConcurrentDictionary<IAsset, byte> InitializeChildren()
        {
            _childrenDictionary = new ConcurrentDictionary<IAsset, byte>();
            return _childrenDictionary;
        }
        
        public bool TryLock(OperationIdentifier owner)
        {
            lock (_ownerLock)
            {
                if (_owner == owner)
                    return true;

                if (_owner != default)
                    return false;

                _owner = owner;
                return true;
            }
        }

        public void Release(OperationIdentifier owner)
        {
            lock (_ownerLock)
            {
                if (_owner == owner)
                    _owner = default;
            }
        }
    }
}