using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons.Units.Weapons
{
    public interface IWeapon : IUnit<IWeaponData, IWeaponController, IWeaponAnimator>
    {
    }

    public class Weapon
        : AbstractUnit<IWeaponData, IWeaponController, IWeaponAnimator>
        , IWeapon
    {
        public Weapon(
            IUserAdapter user,
            IWeaponData data,
            IWeaponController controller,
            IWeaponAnimator animator,
            IEnumerable<IOperationsRunner> operationsRunners)
            : base(user, data, controller, animator, operationsRunners)
        {
        }
    }
}