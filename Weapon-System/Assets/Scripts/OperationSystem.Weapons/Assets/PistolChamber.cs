using OperationSystem.Assets;
using OperationSystem.Component.Types;
using OperationSystem.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Assets
{
    public class PistolChamber 
        : AbstractAsset
        , IAssetSync<Chamber>
    {
        public bool HasRound { get; set;  }

        public PistolChamber(bool hasRound)
        {
            HasRound = hasRound;
        }
        
        public override ComponentMask GetComponentMask()
        {
            var mask = base.GetComponentMask();
            mask.Add<Chamber>();
            return mask;
        }
        
        public void PullInto(ref Chamber component, UnitWorld world)
        {
            component.HasRound = HasRound;
        }

        public void PushFrom(in Chamber component, UnitWorld world)
        {
            HasRound = component.HasRound;
        }
    }
}