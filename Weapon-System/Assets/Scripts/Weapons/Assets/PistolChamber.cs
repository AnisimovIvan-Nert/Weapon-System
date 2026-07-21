using System.Collections.Generic;
using System.Linq;
using Weapons.Units;
using Weapons.Units.Implementations;

namespace Weapons.Assets
{
    public class PistolChamber : IAsset
    {
        public bool HasRound { get; }
        public List<IAsset> Children { get; } = new();
        
        public PistolChamber(bool hasRound)
        {
            HasRound = hasRound;
        }
        
        public IUnit ToUnit()
        {
            var children = Children.Select(o => o.ToUnit()).ToList();
            return new Chamber(HasRound, children);
        }
    }
}