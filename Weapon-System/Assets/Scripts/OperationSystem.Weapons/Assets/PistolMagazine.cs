using System.Collections.Generic;
using OperationSystem.Assets;
using OperationSystem.Component;
using OperationSystem.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Assets
{
    public class PistolMagazine : IAsset
    {
        public int Rounds { get; set;  }
        public List<IAsset> Children { get; } = new();

        public PistolMagazine(int rounds)
        {
            Rounds = rounds;
        }

        public IEnumerable<IAsset> EnumerateUnitChildren() => Children;

        public void CreateComponents(Unit unit, UnitWorld world)
        {
            var magazine = new Magazine(Rounds);
            unit.ComponentsData.AddComponent(magazine);
        }

        public void BeforeUpdate(Unit unit, UnitWorld world)
        {
            var magazine = unit.ComponentsData.Get<IMagazine>();
            magazine.Rounds = Rounds;
        }

        public void AfterUpdate(Unit unit, UnitWorld world)
        {
            var magazine = unit.ComponentsData.Get<IMagazine>();
            
            if (magazine.IsDirty)
                Rounds = magazine.Rounds;
        }
    }
}