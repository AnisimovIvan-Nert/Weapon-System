using OperationSystem.Assets;
using OperationSystem.Handlers;
using OperationSystem.Operations;
using OperationSystem.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.UnitHandlers
{
    public class WeaponUnitHandler : AbstractOperationHandler
    {
        public WeaponUnitHandler(IOperationRunner operationRunner, UnitWorld world) 
            : base(operationRunner, world)
        {
        }
        
        protected override bool IsValidAsset(IAsset asset)
        {
            return asset.GetComponentMask().Contains<Weapon>();
        }
    }
}