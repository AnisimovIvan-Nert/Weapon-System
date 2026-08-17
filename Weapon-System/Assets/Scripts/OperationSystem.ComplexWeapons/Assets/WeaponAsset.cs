using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Component.Types;

namespace OperationSystem.ComplexWeapons.Assets
{
    public class WeaponAsset : AbstractAsset
    {
        public override ComponentMask GetComponentMask()
        {
            var mask = base.GetComponentMask();
            mask.Add<Weapon>();
            return mask;
        }
    }
}