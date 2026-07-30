using OperationSystem.Assets;
using OperationSystem.Component.Types;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Assets
{
    public class PistolMagazine 
        : AbstractAsset
        , IAssetSync<Magazine>
    {
        public int Rounds { get; set;  }

        public PistolMagazine(int rounds)
        {
            Rounds = rounds;
        }

        public override ComponentMask GetComponentMask() => ComponentMask.Create<Magazine>();
        
        public void PullInto(ref Magazine component)
        {
            component.Rounds = Rounds;
        }

        public void PushFrom(in Magazine component)
        {
            Rounds = component.Rounds;
        }
    }
}