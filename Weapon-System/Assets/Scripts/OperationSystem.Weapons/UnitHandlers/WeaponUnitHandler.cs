using OperationSystem.Handlers.Units;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Weapons.UnitHandlers
{
    public class WeaponUnitHandler : AbstractUnitOperationHandler
    {
        public WeaponUnitHandler(IOperationRunner operationRunner, UnitWorld world) 
            : base(operationRunner, world)
        {
        }
    }
}