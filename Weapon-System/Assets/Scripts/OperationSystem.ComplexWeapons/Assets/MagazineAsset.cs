using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Component.Types;
using OperationSystem.Units;

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

        public override ComponentMask GetComponentMask()
        {
            var mask = base.GetComponentMask();
            mask.Add<Magazine>();
            return mask;
        }
        
        public void PullInto(ref Magazine component, UnitWorld world)
        {
            component.Rounds = Rounds;
        }

        public void PushFrom(in Magazine component, UnitWorld world)
        {
            Rounds = component.Rounds;
        }
    }
}