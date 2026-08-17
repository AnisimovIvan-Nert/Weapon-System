using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Components.Handlers.Hit;
using OperationSystem.Component.Types;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Assets
{
    public class PlayerAsset
        : AbstractAsset
        , IAssetPull<HitHandlerComponent>
    {
        public override ComponentMask GetComponentMask()
        {
            var mask = base.GetComponentMask();
            mask.Add<HitHandlerComponent>();
            return mask;
        }
        
        public void PullInto(ref HitHandlerComponent component, UnitWorld world)
        {
            component = new HitHandlerComponent(new PlayerHitHandler());
        }
    }
}