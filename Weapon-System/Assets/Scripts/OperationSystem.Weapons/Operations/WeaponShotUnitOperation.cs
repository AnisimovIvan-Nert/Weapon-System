using System;
using System.Collections;
using System.Collections.Generic;
using OperationSystem.Component;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Units;
using OperationSystem.Weapons.Units;

namespace OperationSystem.Weapons.Operations
{
    public class WeaponShotUnitOperation : AbstractUnitOperation<IWeapon>
    {
        public WeaponShotUnitOperation(
            Guid identifier, 
            IEnumerable<IOperationMiddleware> middlewares,
            params IOperationData[] data) 
            : base(identifier, middlewares, data)
        {
        }

        protected override IEnumerator IncrementEnumerator(IOperationContext context)
        {
            yield return Validate(context);

            yield return AcquireLocks(context);

            var chamberAccess = context.Access<IChamber>(Identifier);
            var magazineAccess = context.TryAccess<IMagazine>(Identifier);

            RecordPossibleMutation(context);

            var chamber = chamberAccess.Component;
            var magazine = magazineAccess?.Component;

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

            chamberAccess.Component = chamber;
            if (magazineAccess != null && magazine != null)
            {
                var magazineAccessValue = magazineAccess.Value;
                magazineAccessValue.Component = magazine;
            }
        }

        private IEnumerator Validate(IOperationContext context)
        {
            var weaponUnit = Handler.Unit ?? throw new InvalidOperationException();
            var componentData = weaponUnit.ComponentsData;

            var chamber = componentData.ReadComponent<IChamber>();
            var magazine = componentData.TryReadComponent<IMagazine>();

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
                    yield break;

                yield return null;
            }

            throw new InvalidOperationException();

            void Acquire()
            {
                var weaponUnit = Handler.Unit ?? throw new InvalidOperationException();
                var componentData = weaponUnit.ComponentsData;
                
                var chamberResource = componentData.GetResource<IChamber>();
                var magazineResource = componentData.TryGetResource<Magazine>();
                
                context.Acquire(chamberResource, Identifier);
                
                if (magazineResource != null)
                    context.Acquire(magazineResource.Value, Identifier);
            }
        }

        private void RecordPossibleMutation(IOperationContext context)
        {
            var chamber = context.Read<IChamber>(Identifier);
            var magazine = context.TryRead<IMagazine>(Identifier);

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