using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Components.Handlers.Hit;
using OperationSystem.Component.Types;

namespace OperationSystem.ComplexWeapons.Assets
{
    public class ObstacleAsset
        : AbstractAsset
        , IAssetPull<HitHandlerComponent>
    {
        public override ComponentMask GetComponentMask()
        {
            return ComponentMask.Create<HitHandlerComponent>();
        }
        
        public void PullInto(ref HitHandlerComponent component)
        {
            component = new HitHandlerComponent(new ObstacleHitHandler());
        }
    }
}