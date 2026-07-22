using OperationSystem.Handlers.Units;
using OperationSystem.Operations;
using OperationSystem.Weapons.Units;

namespace OperationSystem.Weapons.UnitHandlers
{
    public class WeaponUnitHandler : AbstractUnitOperationHandler<IWeapon>
    {
        public WeaponUnitHandler(IOperationRunner operationRunner) 
            : base(operationRunner)
        {
        }
    }
}