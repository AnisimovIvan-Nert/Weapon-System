using System;
using System.Collections;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Result;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Operations
{
    public class WeaponShotOperation : AbstractOperation
    {
        public class MagazineIsEmptyException : OperationException {}
        
        private Unit Weapon => Handler.OperationUnit ?? throw new InvalidOperationException();
        private Unit Magazine => Weapon.GetChild<Magazine>();

        public WeaponShotOperation(
            OperationIdentifier identifier,
            IOperationMiddleware[] middlewares,
            params IOperationData[] data)
            : base(identifier, middlewares, data)
        {
        }

        protected override IEnumerator IncrementEnumerator(IOperationContext context)
        {
            Validate();
            yield return AcquireLocks(context);
            RecordPossibleMutations(context);
            Execute();
        }

        private void Validate()
        {
            var magazine = Magazine.GetComponent<Magazine>();
            if (magazine is not { Rounds: > 0 })
                throw new MagazineIsEmptyException();
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
                context.Acquire<Magazine>(Magazine.Id, Identifier);
            }
        }

        private void RecordPossibleMutations(IOperationContext context)
        {
            context.RecordUndo(Undo);
            return;

            void Undo()
            {
                var magazine = Magazine.GetComponent<Magazine>();
                magazine.Rounds += 1;
                Magazine.SetComponent(magazine);
            }
        }
        
        private void Execute()
        {
            var magazine = Magazine.GetComponent<Magazine>();
            if (magazine is not { Rounds: > 0 })
                throw new MagazineIsEmptyException();

            magazine.Rounds--;
            Magazine.SetComponent(magazine);
        }
    }
}