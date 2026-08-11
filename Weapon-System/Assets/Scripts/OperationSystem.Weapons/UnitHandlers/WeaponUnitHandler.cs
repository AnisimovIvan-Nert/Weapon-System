using OperationSystem.Assets;
using OperationSystem.Handlers;
using OperationSystem.Operations;
using OperationSystem.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.UnitHandlers
{
    public class WeaponUnitHandler : AbstractOperationHandler
    {
        public WeaponUnitHandler(UnitWorld world, IOperationRunner? operationRunner = null) 
            : base(world, operationRunner)
        {
        }
        
        protected override bool IsValidAsset(IAsset asset)
        {
            return asset.GetComponentMask().Contains<Weapon>();
        }
    }
}