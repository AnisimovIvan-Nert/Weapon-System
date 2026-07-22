using System.Collections.Generic;
using System.Linq;
using OperationSystem.Assets;
using OperationSystem.Units;
using OperationSystem.Weapons.Units;

namespace OperationSystem.Weapons.Assets
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