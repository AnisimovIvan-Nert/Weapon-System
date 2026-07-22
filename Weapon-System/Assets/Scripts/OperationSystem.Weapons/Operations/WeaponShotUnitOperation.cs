using System;
using System.Collections;
using OperationSystem.Operations;
using OperationSystem.Operations.Units;
using OperationSystem.Units;
using OperationSystem.Weapons.Units;

namespace OperationSystem.Weapons.Operations
{
    public class WeaponShotUnitOperation : AbstractUnitOperation<IWeapon>
    {
        public WeaponShotUnitOperation(Guid identifier)
            : base(identifier)
        {
        }

        protected override IEnumerator IncrementEnumerator(IOperationContext context)
        {
            yield return Validate(context);

            yield return AcquireLocks(context);

            var chamber = context.AccessFirst<IChamber>(this);
            var magazine = context.TryAccessFirst<IMagazine>(this);

            RecordPossibleMutation(context);

            if (!chamber.HasRound)
            {
                if (magazine == null)
                    throw new InvalidOperationException();

                if (magazine.Rounds <= 0)
                    throw new InvalidOperationException();

                magazine.Rounds--;
                chamber.HasRound = true;
            }

            chamber.HasRound = false;
        }

        private IEnumerator Validate(IOperationContext context)
        {
            var weapon = Handler.Unit ?? throw new InvalidOperationException();

            var chamber = weapon.Find<IChamber>();
            var magazine = weapon.TryFind<IMagazine>();

            if (!chamber.HasRound && magazine is not { Rounds: > 0 })
                throw new InvalidOperationException();

            yield break;
        }

        private IEnumerator AcquireLocks(IOperationContext context)
        {
            var timer = AcquireLocksTimeout;

            while (timer > 0)
            {
                timer--;

                var success = true;
                try
                {
                    Acquire();
                }
                catch (AcquireException)
                {
                    success = false;
                    context.ReleaseAll();
                }
                
                if (success)
                    break;

                yield return null;
            }

            throw new InvalidOperationException();

            void Acquire()
            {
                var weapon = Handler.Unit ?? throw new InvalidOperationException();
                var chamber = weapon.Find<Chamber>();
                
                context.Acquire(chamber, this);

                if (!chamber.HasRound)
                {
                    var magazine = weapon.Find<Magazine>();
                    context.Acquire(magazine, this);
                }
            }
        }

        private void RecordPossibleMutation(IOperationContext context)
        {
            var chamber = context.AccessFirst<IChamber>(this);
            var magazine = context.TryAccessFirst<IMagazine>(this);

            if (magazine != null)
            {
                var magazineRounds = magazine.Rounds;
                context.RecordUndo(() => magazine.Rounds = magazineRounds);
            }

            var hasBullet = chamber.HasRound;
            context.RecordUndo(() => chamber.HasRound = hasBullet);
        }
    }
}