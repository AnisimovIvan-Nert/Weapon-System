using System;
using System.Collections;
using System.Linq;
using OperationSystem.Component;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Operations
{
    public class WeaponShotUnitOperation : AbstractOperation
    {
        private Unit Weapon => this.GetData<IOperationUnit>().Unit;
        private Unit Chamber => Weapon.GetChild<Chamber>();
        private Unit? Magazine => Weapon.TryGetChild<Magazine>();

        private ComponentArray<Chamber> ChamberComponents => Weapon.World.GetComponents<Chamber>();
        private ComponentArray<Magazine> MagazineComponents => Weapon.World.GetComponents<Magazine>();

        public WeaponShotUnitOperation(
            OperationIdentifier identifier,
            IOperationUnit operationUnit,
            IOperationMiddleware[] middlewares)
            : base(identifier, middlewares, operationUnit)
        {
        }

        protected override IEnumerator ValidateEnumerator()
        {
            yield return base.ValidateEnumerator();
            
            var chamber = ChamberComponents.GetComponent(Chamber.Id);
            Magazine? magazine = Magazine != null
                ? MagazineComponents.GetComponent(Magazine.Value.Id) 
                : null;

            if (!chamber.HasRound && magazine is not { Rounds: > 0 })
                throw new InvalidOperationException();
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();
            
            Context.Acquire<Chamber>(Chamber.Id, Identifier);

            if (Magazine != null)
                Context.Acquire<Magazine>(Magazine.Value.Id, Identifier);
        }

        protected override IEnumerator RecordMutationsEnumerator()
        {
            yield return base.RecordMutationsEnumerator();
            
            var chamber = ChamberComponents.GetComponent(Chamber.Id);
            Magazine? magazine = Magazine != null
                ? MagazineComponents.GetComponent(Magazine.Value.Id) 
                : null;

            if (Magazine != null && magazine != null)
                Context.RecordUndo(() => MagazineComponents.SetComponent(Magazine.Value.Id, magazine.Value));

            Context.RecordUndo(() => ChamberComponents.SetComponent(Chamber.Id, chamber));
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var chamber = ChamberComponents.GetComponent(Chamber.Id);
            if (!chamber.HasRound)
            {
                if (Magazine == null)
                    throw new InvalidOperationException();
                
                var magazine = MagazineComponents.GetComponent(Magazine.Value.Id);
                if (magazine is not { Rounds: > 0 })
                    throw new InvalidOperationException();
                
                magazine.Rounds--;
                MagazineComponents.SetComponent(Magazine.Value.Id, magazine);
            }

            chamber.HasRound = false;
            ChamberComponents.SetComponent(Chamber.Id, chamber);
        }
    }
}