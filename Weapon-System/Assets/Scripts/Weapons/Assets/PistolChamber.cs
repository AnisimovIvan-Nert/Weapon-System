using System.Collections.Generic;
using System.Linq;
using Weapons.Units;

namespace Weapons.Assets
{
    public class PistolChamber : IAsset
    {
        public List<IAsset> Children { get; } = new();
        
        public IUnit ToUnit()
        {
            var children = Children.Select(o => o.ToUnit()).ToList();
            return new Chamber(children);
        }
    }
}