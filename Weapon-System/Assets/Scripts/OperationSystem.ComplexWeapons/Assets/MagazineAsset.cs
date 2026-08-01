using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Component.Types;

namespace OperationSystem.ComplexWeapons.Assets
{
    public class MagazineAsset 
        : AbstractAsset
        , IAssetSync<Magazine>
    {
        public int Rounds { get; set;  }

        public MagazineAsset(int rounds)
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