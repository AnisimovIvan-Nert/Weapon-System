using OperationSystem.Handlers.Units;
using OperationSystem.Operations;

namespace OperationSystem.Weapons.UnitHandlers
{
    public class WeaponUnitHandler : AbstractUnitOperationHandler
    {
        public WeaponUnitHandler(IOperationRunner operationRunner) 
            : base(operationRunner)
        {
        }
    }
}