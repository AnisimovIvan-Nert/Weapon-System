using System.Collections.Generic;

namespace Weapons.Units.Implementations
{
    public interface IMagazine : IUnit
    {
    }

    public class Magazine
        : AbstractUnit
        , IMagazine
    {
        public Magazine(IEnumerable<IUnit> children)
            : base(children)
        {
        }
    }
}