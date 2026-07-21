using System.Collections.Generic;
using System.Linq;
using Weapons.Units;
using Weapons.Units.Implementations;

namespace Weapons.Assets
{
    public class Pistol : IAsset
    {
        public List<IAsset> Children { get; } = new();
        
        public IUnit ToUnit()
        {
            var children = Children.Select(o => o.ToUnit()).ToList();
            return new Weapon(children);
        }
    }
}