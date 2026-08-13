using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Component.Types;

namespace OperationSystem.ComplexWeapons.Assets
{
    public class BarrelAsset 
        : AbstractAsset
        , IAssetPull<Barrel>
    {
        public override ComponentMask GetComponentMask() => ComponentMask.Create<Barrel>();
        
        public void PullInto(ref Barrel component)
        {
            component = new Barrel(transform.forward.normalized, transform.position);
        }
    }
}