using System.Collections.Generic;

namespace Weapons.Units
{
    public interface IWeapon : IUnit
    {
    }

    public class Weapon
        : AbstractUnit
        , IWeapon
    {
        public Weapon(IEnumerable<IUnit> children)
            : base(children)
        {
        }
    }
}