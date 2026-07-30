using System;
using System.Collections;
using System.Collections.Generic;
using OperationSystem.Component;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Units;
using OperationSystem.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Operations
{
    public class WeaponShotUnitOperation : AbstractUnitOperation<IWeapon>
    {
        private Unit WeaponUnit => Handler.Unit ?? throw new InvalidOperationException();
        private UnitWorld World => WeaponUnit.World;

        private ComponentArray<Chamber> ChamberComponents => World.GetComponents<Chamber>();
        private ComponentArray<Magazine> MagazineComponents => World.GetComponents<Magazine>();

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

            RecordPossibleMutation(context);

            var chamber = ChamberComponents.GetComponent(WeaponUnit.Id);
            if (!chamber.HasRound)
            {
                var nullableMagazine = MagazineComponents.TryGetComponent(WeaponUnit);
                if (nullableMagazine is not { Rounds: > 0 })
                    throw new InvalidOperationException();

                var magazine = nullableMagazine.Value;
                magazine.Rounds--;
                MagazineComponents.SetComponent(WeaponUnit.Id, magazine);
            }

            chamber.HasRound = false;
            ChamberComponents.SetComponent(WeaponUnit.Id, chamber);
        }

        private IEnumerator Validate(IOperationContext context)
        {
            var chamber = ChamberComponents.GetComponent(WeaponUnit.Id);
            var magazine = MagazineComponents.TryGetComponent(WeaponUnit);

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
                context.Acquire<Chamber>(WeaponUnit.Id, Identifier);

                if (MagazineComponents.HasComponent(WeaponUnit))
                    context.Acquire<Magazine>(WeaponUnit.Id, Identifier);
            }
        }

        private void RecordPossibleMutation(IOperationContext context)
        {
            var chamber = ChamberComponents.GetComponent(WeaponUnit.Id);
            var magazine = MagazineComponents.TryGetComponent(WeaponUnit);

            if (magazine != null)
                context.RecordUndo(() => MagazineComponents.SetComponent(WeaponUnit.Id, magazine.Value));

            context.RecordUndo(() => ChamberComponents.SetComponent(WeaponUnit.Id, chamber));
        }
    }
}