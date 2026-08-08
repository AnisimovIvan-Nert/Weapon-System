using OperationSystem.Assets;
using OperationSystem.Component.Types;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Assets
{
    public class Pistol : AbstractAsset
    {
        public override ComponentMask GetComponentMask() => ComponentMask.Create<Weapon>();
    }
}