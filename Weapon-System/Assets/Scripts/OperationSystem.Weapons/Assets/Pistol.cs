using System.Collections.Generic;
using System.Linq;
using OperationSystem.Assets;
using OperationSystem.Units;
using OperationSystem.Weapons.Units;

namespace OperationSystem.Weapons.Assets
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