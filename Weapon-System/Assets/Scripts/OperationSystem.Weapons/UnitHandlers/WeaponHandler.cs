using OperationSystem.Operations;
using OperationSystem.UnitHandlers;
using OperationSystem.Weapons.Units;

namespace OperationSystem.Weapons.UnitHandlers
{
    public class WeaponHandler : AbstractUnitHandler<IWeapon>
    {
        public WeaponHandler(IOperationRunner operationRunner) 
            : base(operationRunner)
        {
        }
    }
}