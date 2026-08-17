using System.Collections.Generic;
using System.Collections.Immutable;
using OperationSystem.Component;

namespace OperationSystem.Units.Child
{
    public struct ChildrenComponent : IComponent
    {
        private ChildrenReference _childrenReference;
        public IEnumerable<Unit> Children => _childrenReference.Value;

        public void SetChildren(params Unit[] children)
        {
            _childrenReference ??= new ChildrenReference();
            _childrenReference.Value = ImmutableArray.Create(children);
        }
        
        private class ChildrenReference
        {
            public ImmutableArray<Unit> Value { get; set; }
        }
    }
}