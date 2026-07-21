using System.Collections.Generic;

namespace Weapons.Units.Weapons.Controller
{
    public interface IWeaponController : IComplexUnitController
    {
        IWeaponShotOperationController ShotController { get; }
    }

    public class WeaponController : IWeaponController
    {
        public IWeaponShotOperationController ShotController { get; }

        public IEnumerable<IOperationController> OperationControllers => new[] { ShotController };

        private WeaponController(IWeaponShotOperationController shotController)
        {
            ShotController = shotController;
        }

        public static WeaponController Create()
        {
            var shotController = new WeaponShotOperationController();
            return new WeaponController(shotController);
        }
    }
}