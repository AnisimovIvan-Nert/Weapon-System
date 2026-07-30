using OperationSystem.Assets;
using OperationSystem.Component.Types;

namespace OperationSystem.Weapons.Assets
{
    public class Pistol : AbstractAsset
    {
        public override ComponentMask GetComponentMask() => ComponentMask.Create();
    }
}