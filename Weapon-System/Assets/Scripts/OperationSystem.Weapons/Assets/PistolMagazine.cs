using OperationSystem.Assets;

namespace OperationSystem.Weapons.Assets
{
    public class PistolMagazine : AbstractAsset
    {
        public int rounds;

        public PistolMagazine(int rounds)
        {
            this.rounds = rounds;
        }
    }
}