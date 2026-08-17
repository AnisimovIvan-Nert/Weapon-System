using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OperationSystem.Component.Types;
using OperationSystem.Units;
using OperationSystem.Units.Child;
using UnityEngine;

namespace OperationSystem.Assets
{
    public abstract class AbstractAsset 
        : MonoBehaviour
        , IAsset
        , IAssetPull<ChildrenComponent>
    {
        private ConcurrentDictionary<IAsset, byte>? _childrenDictionary;
        private ConcurrentDictionary<IAsset, byte> ChildrenDictionary => _childrenDictionary ?? InitializeChildren();

        public virtual IEnumerable<IAsset> Children => ChildrenDictionary.Keys;
        
        public virtual bool TryAddChild(IAsset child) => ChildrenDictionary.TryAdd(child, 0);
        public virtual bool TryRemoveChild(IAsset child) => ChildrenDictionary.TryRemove(child, out _);
        
        public virtual void PullInto(ref ChildrenComponent component, UnitWorld world)
        {
            var children = Children.Select(world.GetOrCreateUnit).ToArray();
            component.SetChildren(children);
        }

        public virtual ComponentMask GetComponentMask() => ComponentMask.Create<ChildrenComponent>();

        private ConcurrentDictionary<IAsset, byte> InitializeChildren()
        {
            _childrenDictionary = new ConcurrentDictionary<IAsset, byte>();
            return _childrenDictionary;
        }
    }
}