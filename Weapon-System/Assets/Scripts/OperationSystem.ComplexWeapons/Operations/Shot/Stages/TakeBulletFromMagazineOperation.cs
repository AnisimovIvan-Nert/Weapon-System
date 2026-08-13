using System.Collections;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Result;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Operations.Shot.Stages
{

    public class TakeBulletFromMagazineOperation : AbstractOperation
    {
        public class MagazineIsEmptyException : OperationException
        {
        }

        private Unit Weapon => this.GetData<IOperationUnit>().Unit;
        private Unit Magazine => Weapon.GetChild<Magazine>();

        public TakeBulletFromMagazineOperation(OperationIdentifier identifier, IOperationUnit operationUnit)
            : base(identifier, operationUnit)
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

            Context.Acquire<Magazine>(Magazine, Identifier);
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
            
            var magazine = Magazine.GetComponent<Magazine>(Context.World);
            magazine.Rounds--;
            Magazine.SetComponent(magazine, Context.World);
        }
        
        private void Validate()
        {
            var magazine = Magazine.GetComponent<Magazine>(Context.World);
            if (magazine is not { Rounds: > 0 })
                throw new MagazineIsEmptyException();
        }

        private void RecordMutationUndo()
        {
            Context.RecordUndo(() =>
            {
                var magazine = Magazine.GetComponent<Magazine>(Context.World);
                magazine.Rounds++;
                Magazine.SetComponent(magazine, Context.World);
            });
        }
    }
}