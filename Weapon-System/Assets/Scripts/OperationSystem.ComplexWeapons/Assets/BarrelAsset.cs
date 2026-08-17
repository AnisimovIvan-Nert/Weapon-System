using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Component.Types;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Assets
{
    public class BarrelAsset 
        : AbstractAsset
        , IAssetPull<Barrel>
    {
        public override ComponentMask GetComponentMask()
        {
            var mask = base.GetComponentMask();
            mask.Add<Barrel>();
            return mask;
        }
        
        public void PullInto(ref Barrel component, UnitWorld world)
        {
            component = new Barrel(transform.forward.normalized, transform.position);
        }
    }
}