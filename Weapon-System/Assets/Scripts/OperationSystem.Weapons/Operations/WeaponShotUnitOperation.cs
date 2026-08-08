using System;
using System.Collections;
using OperationSystem.Component;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Units;
using OperationSystem.Weapons.Components;

namespace OperationSystem.Weapons.Operations
{
    public class WeaponShotUnitOperation : AbstractOperation
    {
        private Unit Weapon => Handler.OperationUnit ?? throw new InvalidOperationException();
        private Unit Chamber => Weapon.GetChild<Chamber>();
        private Unit? Magazine => Weapon.TryGetChild<Magazine>();

        private ComponentArray<Chamber> ChamberComponents => Weapon.World.GetComponents<Chamber>();
        private ComponentArray<Magazine> MagazineComponents => Weapon.World.GetComponents<Magazine>();

        public WeaponShotUnitOperation(
            OperationIdentifier identifier,
            IOperationMiddleware[] middlewares,
            params IOperationData[] data)
            : base(identifier, middlewares, data)
        {
        }

        protected override IEnumerator IncrementEnumerator(IOperationContext context)
        {
            yield return Validate(context);

            yield return AcquireLocks(context);

            RecordPossibleMutation(context);

            var chamber = ChamberComponents.GetComponent(Chamber.Id);
            if (!chamber.HasRound)
            {
                if (Magazine == null)
                    throw new InvalidOperationException();
                
                var magazine = MagazineComponents.GetComponent(Magazine.Value.Id);
                if (magazine is not { Rounds: > 0 })
                    throw new InvalidOperationException();
                
                magazine.Rounds--;
                MagazineComponents.SetComponent(Magazine.Value.Id, magazine);
            }

            chamber.HasRound = false;
            ChamberComponents.SetComponent(Chamber.Id, chamber);
        }

        private IEnumerator Validate(IOperationContext context)
        {
            var chamber = ChamberComponents.GetComponent(Chamber.Id);
            Magazine? magazine = Magazine != null
                ? MagazineComponents.GetComponent(Magazine.Value.Id) 
                : null;

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
                context.Acquire<Chamber>(Chamber.Id, Identifier);

                if (Magazine != null)
                    context.Acquire<Magazine>(Magazine.Value.Id, Identifier);
            }
        }

        private void RecordPossibleMutation(IOperationContext context)
        {
            var chamber = ChamberComponents.GetComponent(Chamber.Id);
            Magazine? magazine = Magazine != null
                ? MagazineComponents.GetComponent(Magazine.Value.Id) 
                : null;

            if (Magazine != null && magazine != null)
                context.RecordUndo(() => MagazineComponents.SetComponent(Magazine.Value.Id, magazine.Value));

            context.RecordUndo(() => ChamberComponents.SetComponent(Chamber.Id, chamber));
        }
    }
}