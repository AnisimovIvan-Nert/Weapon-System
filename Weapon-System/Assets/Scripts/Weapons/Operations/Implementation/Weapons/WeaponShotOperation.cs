using System;
using System.Collections;
using Weapons.Units;
using Weapons.Units.Implementations;

namespace Weapons.Operations.Implementation.Weapons
{
    public class WeaponShotOperation : AbstractOperation<IWeapon>
    {
        public WeaponShotOperation(Guid identifier)
            : base(identifier)
        {
        }

        protected override IEnumerator IncrementEnumerator(IOperationContext context)
        {
            yield return Validate(context);

            yield return AcquireLocks(context);

            var chamber = context.TryAccessFirst<IChamber>(this) ?? throw new InvalidOperationException();
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

            var chamber = weapon.TryFind<IChamber>();
            var magazine = weapon.TryFind<IMagazine>();

            if (chamber == null)
                throw new InvalidOperationException();

            if (!chamber.HasRound && magazine is not { Rounds: > 0 })
                throw new InvalidOperationException();
            
            yield break;
        }

        private IEnumerator AcquireLocks(IOperationContext context)
        {
            var timer = AcquireLocksTimeout;
            var weapon = Handler.Unit ?? throw new InvalidOperationException();

            while (timer > 0)
            {
                timer--;

                var chamber = weapon.TryFind<Chamber>() ?? throw new InvalidOperationException();

                if (!context.TryAcquire(chamber, this))
                {
                    yield return null;
                    continue;
                }

                if (!chamber.HasRound)
                {
                    var magazine = weapon.TryFind<Magazine>() ?? throw new InvalidOperationException();
                    if (!context.TryAcquire(magazine, this))
                    {
                        context.ReleaseAll();
                        yield return null;
                        continue;
                    }
                }

                break;
            }

            throw new InvalidOperationException();
        }

        private void RecordPossibleMutation(IOperationContext context)
        {
            var chamber = context.TryAccessFirst<IChamber>(this) ?? throw new InvalidOperationException();
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