using System.Collections.Generic;
using OperationSystem.Units;

namespace OperationSystem.Weapons.Units
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