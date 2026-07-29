using System.Collections.Generic;
using OperationSystem.Assets;
using OperationSystem.Component;
using OperationSystem.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Assets
{
    public class PistolChamber : AbstractAsset
    {
        public bool HasRound { get; set; }

        public PistolChamber(bool hasRound)
        {
            HasRound = hasRound;
        }

        public override IEnumerable<IComponentHandle> EnumerateComponents(UnitWorld unitWorld)
        {
            foreach (var component in base.EnumerateComponents(unitWorld))
                yield return component;

            var chamber = new Chamber(HasRound);
            yield return new ComponentHandle<Chamber>(chamber, this);
        }

        public override T PullData<T>(T component, UnitWorld unitWorld)
        {
            switch (component)
            {
                case Chamber chamber:
                    chamber.HasRound = HasRound;
                    return component;
            }
            
            return base.PullData(component, unitWorld);
        }

        public override T PushData<T>(T component, UnitWorld unitWorld)
        {
            return base.PushData(component, unitWorld);
        }
    }
}