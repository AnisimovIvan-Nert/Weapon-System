using System;
using System.Collections;
using OperationSystem.Component;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Operations
{
    public class WeaponShotUnitOperation : AbstractOperation
    {
        private Unit Weapon => this.GetData<IOperationUnit>().Unit;
        private Unit Chamber => Weapon.GetChild<Chamber>(Context.World);
        private Unit? Magazine => Weapon.TryGetChild<Magazine>(Context.World);

        private ComponentArray<Chamber> ChamberComponents => Context.World.GetComponentArray<Chamber>();
        private ComponentArray<Magazine> MagazineComponents => Context.World.GetComponentArray<Magazine>();

        public WeaponShotUnitOperation(OperationIdentifier identifier, IOperationUnit operationUnit)
            : base(identifier, operationUnit)
        {
        }

        protected override IEnumerator ValidateEnumerator()
        {
            yield return base.ValidateEnumerator();

            var chamber = ChamberComponents.GetComponent(Chamber);
            Magazine? magazine = Magazine != null
                ? MagazineComponents.GetComponent(Magazine.Value)
                : null;

            if (!chamber.HasRound && magazine is not { Rounds: > 0 })
                throw new InvalidOperationException();
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();

            Context.Acquire<Chamber>(Chamber, Identifier);

            if (Magazine != null)
                Context.Acquire<Magazine>(Magazine.Value, Identifier);
        }

        protected override IEnumerator RecordMutationsEnumerator()
        {
            yield return base.RecordMutationsEnumerator();

            var chamber = ChamberComponents.GetComponent(Chamber);
            Magazine? magazine = Magazine != null
                ? MagazineComponents.GetComponent(Magazine.Value)
                : null;

            if (Magazine != null && magazine != null)
                Context.RecordUndo(() => MagazineComponents.SetComponent(Magazine.Value, magazine.Value));

            Context.RecordUndo(() => ChamberComponents.SetComponent(Chamber, chamber));
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();

            var chamber = ChamberComponents.GetComponent(Chamber);
            if (!chamber.HasRound)
            {
                if (Magazine == null)
                    throw new InvalidOperationException();

                var magazine = MagazineComponents.GetComponent(Magazine.Value);
                if (magazine is not { Rounds: > 0 })
                    throw new InvalidOperationException();

                magazine.Rounds--;
                MagazineComponents.SetComponent(Magazine.Value, magazine);
            }

            chamber.HasRound = false;
            ChamberComponents.SetComponent(Chamber, chamber);
        }
    }
}