using System;
using System.Collections;
using System.Collections.Generic;
using OperationSystem.Component;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Operations
{
    public class WeaponShotUnitOperation : AbstractUnitOperation<IWeapon>
    {
        public WeaponShotUnitOperation(
            OperationIdentifier identifier, 
            IEnumerable<IOperationMiddleware> middlewares,
            params IOperationData[] data) 
            : base(identifier, middlewares, data)
        {
        }

        protected override IEnumerator IncrementEnumerator(IOperationContext context)
        {
            yield return Validate(context);

            yield return AcquireLocks(context);

            var chamberResource = context.Access<IChamber>(Identifier);
            var magazineResource = context.TryAccess<IMagazine>(Identifier);

            RecordPossibleMutation(context);

            var chamber = chamberResource.Read<IChamber>();
            var magazine = magazineResource?.Read<IMagazine>();
            {
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
            chamberResource.Write(chamber);
            if (magazine != null) 
                magazineResource?.Write(magazine);
        }

        private IEnumerator Validate(IOperationContext context)
        {
            var weaponUnit = Handler.Unit ?? throw new InvalidOperationException();
            var componentData = weaponUnit.ComponentsData;

            var chamber = componentData.Read<IChamber>();
            var magazine = componentData.TryRead<IMagazine>();

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
                
                var chamber = componentData.Get<IChamber>();
                var magazine = componentData.TryGet<Magazine>();
                
                context.Acquire(chamber, Identifier);
                
                if (magazine != null)
                    context.Acquire(magazine, Identifier);
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