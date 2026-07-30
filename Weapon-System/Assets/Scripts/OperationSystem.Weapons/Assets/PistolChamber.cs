using OperationSystem.Assets;
using OperationSystem.Component.Types;
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

        public override ComponentMask GetComponentMask() => ComponentMask.Create<Chamber>();
        
        public void PullInto(ref Chamber component)
        {
            component.HasRound = HasRound;
        }

        public void PushFrom(in Chamber component)
        {
            HasRound = component.HasRound;
        }
    }
}