using System;
using System.Collections;
using System.Collections.Generic;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Result;
using OperationSystem.Operations.Units;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Operations
{
    public class WeaponShotCalculationOperation : AbstractStagedUnitOperation
    {
        public class MagazineIsEmptyException : OperationException {}
        
        private Unit Weapon => Handler.Unit ?? throw new InvalidOperationException();
        private Unit Magazine => Weapon.GetChild<Magazine>();

        public WeaponShotCalculationOperation(
            OperationIdentifier identifier,
            IEnumerable<IOperationMiddleware> middlewares,
            params IOperationData[] data)
            : base(identifier, middlewares, data)
        {
        }

        protected override IEnumerator ValidateEnumerator()
        {
            yield return base.ValidateEnumerator();
            
            var magazine = Magazine.GetComponent<Magazine>();
            if (magazine is not { Rounds: > 0 })
                throw new MagazineIsEmptyException();
        }
        
        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();
            
            Context.Acquire<Magazine>(Magazine.Id, Identifier);
        }

        protected override IEnumerator RecordPossibleMutationsEnumerator()
        {
            yield return base.RecordPossibleMutationsEnumerator();
            
            Context.RecordUndo(Undo);
            yield break;

            void Undo()
            {
                var magazine = Magazine.GetComponent<Magazine>();
                magazine.Rounds += 1;
                Magazine.SetComponent(magazine);
            }
        }
        
        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var magazine = Magazine.GetComponent<Magazine>();
            if (magazine is not { Rounds: > 0 })
                throw new MagazineIsEmptyException();

            magazine.Rounds--;
            Magazine.SetComponent(magazine);
        }
    }
}