using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Handlers;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.UnitHandlers
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