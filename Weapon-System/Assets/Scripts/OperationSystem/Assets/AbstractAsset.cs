using System.Collections.Generic;
using OperationSystem.Component.Types;
using UnityEngine;

namespace OperationSystem.Assets
{
    public abstract class AbstractAsset : MonoBehaviour, IAsset
    {
        protected List<IAsset> ChildrenList = new();

        public virtual IEnumerable<IAsset> Children => ChildrenList;
        
        public virtual void AddChild(IAsset child) => ChildrenList.Add(child);
        public virtual void RemoveChild(IAsset child) => ChildrenList.Remove(child);
        
        public abstract ComponentMask GetComponentMask();
    }
}