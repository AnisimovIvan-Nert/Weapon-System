using System;
using System.Collections;
using System.Threading;
using OperationSystem.Assets;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Weapons.Assets;

namespace OperationSystem.Weapons.Operations
{
    public class WeaponShotUnitOperation : AbstractOperation
    {
        private Pistol Weapon => (Pistol)this.GetData<IOperationAsset>().Asset;
        private PistolChamber Chamber => Weapon.GetChild<PistolChamber>();
        private PistolMagazine? Magazine => Weapon.TryGetChild<PistolMagazine>();

        public WeaponShotUnitOperation(OperationIdentifier identifier, IOperationAsset asset)
            : base(identifier, asset)
        {
        }

        protected override IEnumerator ValidateEnumerator()
        {
            yield return base.ValidateEnumerator();

            if (!Chamber.hasRound && Magazine is not { rounds: > 0 })
                throw new InvalidOperationException();
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();

            Context.Acquire(Chamber, Identifier);

            if (Magazine != null)
                Context.Acquire(Magazine, Identifier);
        }

        protected override IEnumerator RecordMutationsEnumerator()
        {
            yield return base.RecordMutationsEnumerator();

            var magazine = Magazine;
            if (magazine != null)
                Context.RecordUndo(() => Interlocked.Increment(ref magazine.rounds));

            var chamber = Chamber;
            var hasBullet = chamber.hasRound;
            Context.RecordUndo(() => chamber.hasRound = hasBullet);
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();

            var chamber = Chamber;
            if (!chamber.hasRound)
            {
                if (Magazine == null)
                    throw new InvalidOperationException();

                var magazine = Magazine;
                if (magazine is not { rounds: > 0 })
                    throw new InvalidOperationException();

                Interlocked.Decrement(ref magazine.rounds);
            }

            chamber.hasRound = false;
        }
    }
}