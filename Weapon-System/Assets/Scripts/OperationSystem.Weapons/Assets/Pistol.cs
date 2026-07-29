using System.Collections.Generic;
using OperationSystem.Assets;
using OperationSystem.Units;

namespace OperationSystem.Weapons.Assets
{
    public class Pistol : IAsset
    {
        public List<IAsset> Children { get; } = new();
        
        public IEnumerable<IAsset> EnumerateUnitChildren() => Children;
        
        public void CreateComponents(Unit unit, UnitWorld world)
        {
        }

        public void BeforeUpdate(Unit unit, UnitWorld world)
        {
        }

        public void AfterUpdate(Unit unit, UnitWorld world)
        {
        }
    }
}