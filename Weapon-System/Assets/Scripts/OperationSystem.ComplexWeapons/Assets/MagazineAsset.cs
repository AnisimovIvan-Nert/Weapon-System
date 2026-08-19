using OperationSystem.Assets;

namespace OperationSystem.ComplexWeapons.Assets
{
    public class MagazineAsset : AbstractAsset
    {
        public int rounds;

        public MagazineAsset(int rounds)
        {
            this.rounds = rounds;
        }
    }
}