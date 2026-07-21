using System.Collections.Generic;

namespace Weapons.Units
{
    public abstract class AbstractUnit : IUnit
    {
        public IEnumerable<IUnit> Children { get; }

        protected AbstractUnit(IEnumerable<IUnit> children)
        {
            Children = children;
        }
    }
}