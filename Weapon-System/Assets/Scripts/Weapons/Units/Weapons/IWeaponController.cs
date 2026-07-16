using System.Threading.Tasks;
using Weapons.Operations;

namespace Weapons.Units.Weapons
{
    public interface IWeaponController : IUnitController
    {
        Task PerformShot(IWeapon unit, IOperation<IWeapon> operation);
        Task CancelShot(IWeapon unit, IOperation<IWeapon> operation);
    }
}