using OperationSystem.Assets;

namespace OperationSystem.Weapons.Assets
{
    public class PistolChamber : AbstractAsset
    {
        public bool hasRound;

        public PistolChamber(bool hasRound)
        {
            this.hasRound = hasRound;
        }
    }
}