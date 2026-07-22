using Weapons.Operations;
using Weapons.Units.Implementations;

namespace Weapons.UnitHandlers.Implementation
{
    public class WeaponHandler : AbstractUnitHandler<IWeapon>
    {
        public WeaponHandler(IOperationRunner operationRunner) 
            : base(operationRunner)
        {
        }
    }
}