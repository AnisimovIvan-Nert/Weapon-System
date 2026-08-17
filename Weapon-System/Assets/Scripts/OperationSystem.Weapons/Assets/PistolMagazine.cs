using OperationSystem.Assets;
using OperationSystem.Component.Types;
using OperationSystem.Units;
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