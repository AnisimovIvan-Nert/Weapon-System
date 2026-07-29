using System.Collections.Generic;
using OperationSystem.Assets;
using OperationSystem.Component;
using OperationSystem.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Assets
{
    public class PistolChamber : IAsset
    {
        public bool HasRound { get; set; }
        public List<IAsset> Children { get; } = new();
        
        public PistolChamber(bool hasRound)
        {
            HasRound = hasRound;
        }
        
        public IEnumerable<IAsset> EnumerateUnitChildren() => Children;
        
        public void CreateComponents(Unit unit, UnitWorld world)
        {
            var chamber = new Chamber(HasRound);
            unit.ComponentsData.AddComponent(chamber);
        }

        public void BeforeUpdate(Unit unit, UnitWorld world)
        {
            var chamber = unit.ComponentsData.Get<IChamber>();
            chamber.HasRound = HasRound;
        }

        public void AfterUpdate(Unit unit, UnitWorld world)
        {
            var chamber = unit.ComponentsData.Get<IChamber>();
            
            if (chamber.IsDirty)
                HasRound = chamber.HasRound;
        }
    }
}