using System.Collections.Generic;

namespace Weapons.Units.Implementations
{
    public interface IChamber : IUnit
    {
    }

    public class Chamber
        : AbstractUnit
        , IChamber
    {
        public Chamber(IEnumerable<IUnit> children)
            : base(children)
        {
        }
    }
}