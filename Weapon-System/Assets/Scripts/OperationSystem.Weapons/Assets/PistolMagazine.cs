using System.Collections.Generic;
using System.Linq;
using OperationSystem.Assets;
using OperationSystem.Units;
using OperationSystem.Weapons.Units;

namespace OperationSystem.Weapons.Assets
{
    public class PistolMagazine : IAsset
    {
        public int Rounds { get; }
        public List<IAsset> Children { get; } = new();
        
        public PistolMagazine(int rounds)
        {
            Rounds = rounds;
        }
        
        public IUnit ToUnit()
        {
            var children = Children.Select(o => o.ToUnit()).ToList();
            return new Magazine(Rounds, children);
        }
    }
}