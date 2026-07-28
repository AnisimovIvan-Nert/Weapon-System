using System.Collections.Generic;
using OperationSystem.Units;

namespace OperationSystem.Component
{
    public interface IChildrenComponent : IComponent
    {
        IEnumerable<UnitId> Children { get; }
    }
    
    public class ChildrenComponent : AbstractComponent, IChildrenComponent
    {
        public IEnumerable<UnitId> Children { get; }
        
        public ChildrenComponent(params UnitId[] children)
        {
            Children = children;
        }
    }
}