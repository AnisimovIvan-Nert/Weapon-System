using System;
using System.Collections;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
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

        private Unit Weapon => Handler.MainUnit ?? throw new InvalidOperationException();
        private Unit Magazine => Weapon.GetChild<Magazine>();

        public WeaponShotOperation(OperationIdentifier identifier, IOperationMiddleware[] middlewares)
            : base(identifier, middlewares)
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
            var magazine = Magazine.GetComponent<Magazine>();
            if (magazine is not { Rounds: > 0 })
                throw new MagazineIsEmptyException();
        }

        private void PerformMutation()
        {
            var magazine = Magazine.GetComponent<Magazine>();
            magazine.Rounds--;
            Magazine.SetComponent(magazine);
        }

        private void RecordMutationUndo()
        {
            var magazine = Magazine.GetComponent<Magazine>();
            var rounds = magazine.Rounds;

            Context.RecordUndo(() =>
            {
                Magazine.GetComponent<Magazine>();
                magazine.Rounds = rounds;
                Magazine.SetComponent(magazine);
            });
        }
    }
}