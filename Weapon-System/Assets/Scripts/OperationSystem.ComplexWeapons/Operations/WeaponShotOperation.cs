using System.Collections;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Result;
using OperationSystem.Operations.Tags;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Operations
{
    public interface IWeaponShotOperationTag : IOperationTag
    {
    }

    public class WeaponShotOperation : AbstractOperation, IWeaponShotOperationTag
    {
        public class MagazineIsEmptyException : OperationException
        {
        }

        private Unit Weapon => this.GetData<IOperationUnit>().Unit;
        private Unit Magazine => Weapon.GetChild<Magazine>();

        public WeaponShotOperation(
            OperationIdentifier identifier, 
            IOperationUnit operationUnit, 
            IOperationMiddleware[] middlewares)
            : base(identifier, middlewares, operationUnit)
        {
        }

        protected override IEnumerator ValidateEnumerator()
        {
            yield return base.ValidateEnumerator();

            Validate();
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();

            Context.Acquire<Magazine>(Magazine.Id, Identifier);
        }

        protected override IEnumerator RecordMutationsEnumerator()
        {
            yield return base.RecordMutationsEnumerator();

            RecordMutationUndo();
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();

            Validate();
            PerformMutation();
        }

        private void Validate()
        {
            var magazine = Magazine.GetComponent<Magazine>(Context.World);
            if (magazine is not { Rounds: > 0 })
                throw new MagazineIsEmptyException();
        }

        private void PerformMutation()
        {
            var magazine = Magazine.GetComponent<Magazine>(Context.World);
            magazine.Rounds--;
            Magazine.SetComponent(magazine, Context.World);
        }

        private void RecordMutationUndo()
        {
            var magazine = Magazine.GetComponent<Magazine>(Context.World);
            var rounds = magazine.Rounds;

            Context.RecordUndo(() =>
            {
                Magazine.GetComponent<Magazine>(Context.World);
                magazine.Rounds = rounds;
                Magazine.SetComponent(magazine, Context.World);
            });
        }
    }
}