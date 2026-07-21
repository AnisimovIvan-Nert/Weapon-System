using Weapons.Operations;
using Weapons.Units.Implementations;

namespace Weapons.UnitHandlers.Implementation
{
    public class WeaponHandler : AbstractUnitHandler<IWeapon>
    {
        public WeaponHandler(IWeapon unit, IOperationRunner operationRunner) 
            : base(unit, operationRunner)
        {
        }
    }
}