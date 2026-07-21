using System.Collections.Generic;
using Weapons.Resource;

namespace Weapons.Units
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