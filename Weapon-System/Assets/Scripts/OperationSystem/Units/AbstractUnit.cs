using System.Collections.Generic;
using OperationSystem.Resource;

namespace OperationSystem.Units
{
    public abstract class AbstractUnit : AbstractResource, IUnit
    {
        public IEnumerable<IUnit> Children { get; }

        protected AbstractUnit(IEnumerable<IUnit> children)
        {
            Children = children;
        }
    }
}