using System;
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
        
        public void CreateComponents(Unit unit, UnitWorld world)
        {
            var magazine = new Magazine(Rounds);
            unit.ComponentsData.AddComponent(magazine);
        }

        public void ReadComponents(Unit unit, UnitWorld world)
        {
            throw new System.NotImplementedException();
        }

        public void SetComponents(Unit unit, UnitWorld world)
        {
            var data = new Span<byte>()
        }
        
        public Unit ToUnit()
        {
            var children = Children.Select(o => o.ToUnit()).ToList();
            return new Magazine(Rounds, children);
        }
    }
}