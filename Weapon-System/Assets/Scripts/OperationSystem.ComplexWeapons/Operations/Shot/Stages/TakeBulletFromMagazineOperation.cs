using System.Collections;
using System.Threading;
using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Assets;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Result;

namespace OperationSystem.ComplexWeapons.Operations.Shot.Stages
{

    public class TakeBulletFromMagazineOperation : AbstractOperation
    {
        public class MagazineIsEmptyException : OperationException
        {
        }

        private IAsset Weapon => this.GetData<IOperationAsset>().Asset;
        private MagazineAsset Magazine => Weapon.GetChild<MagazineAsset>();

        public TakeBulletFromMagazineOperation(OperationIdentifier identifier, IOperationAsset operationAsset)
            : base(identifier, operationAsset)
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

            Context.Acquire(Magazine, Identifier);
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

            var magazine = Magazine;
            Interlocked.Decrement(ref magazine.rounds);
        }
        
        private void Validate()
        {
            if (Magazine is not { rounds: > 0 })
                throw new MagazineIsEmptyException();
        }

        private void RecordMutationUndo()
        {
            var magazine = Magazine;
            Context.RecordUndo(() =>
            {
                Interlocked.Increment(ref magazine.rounds);
            });
        }
    }
}