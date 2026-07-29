using System.Collections.Generic;
using System.Linq;
using OperationSystem.Units;

namespace OperationSystem.Component
{
    public interface IChildrenComponent : IComponent
    {
        IList<UnitId> Children { get; }
    }
    
    public readonly struct ChildrenComponent : IChildrenComponent
    {
        public IList<UnitId> Children { get; }
        
        public ChildrenComponent(params UnitId[] children)
        {
            Children = children.ToList();
        }
    }
}