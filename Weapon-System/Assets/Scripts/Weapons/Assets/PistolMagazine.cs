using System.Collections.Generic;
using System.Linq;
using Weapons.Units;
using Weapons.Units.Implementations;

namespace Weapons.Assets
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