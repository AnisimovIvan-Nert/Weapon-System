using System.Collections;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.ComplexWeapons.Components.Controllers.Hit;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Result;
using OperationSystem.Operations.Tags;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Operations
{
    public interface IWeaponShotOperationTag : IOperationTag
    {
    }

    public class WeaponShotOperation : AbstractOperation, IWeaponShotOperationTag
    {
        public class MagazineIsEmptyException : OperationException
        {
        }

        private Unit Weapon => this.GetData<IOperationUnit>().Unit;
        private Unit Magazine => Weapon.GetChild<Magazine>();
        private Unit Barrel => Weapon.GetChild<Barrel>();

        public WeaponShotOperation(
            OperationIdentifier identifier, 
            Data data,
            IOperationUnit operationUnit, 
            IOperationMiddleware[] middlewares)
            : base(identifier, middlewares, operationUnit, data)
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
            Context.Acquire<Barrel>(Barrel, Identifier);
        }

        protected override IEnumerator RecordMutationsEnumerator()
        {
            yield return base.RecordMutationsEnumerator();

            RecordMutationUndo();
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var data = this.GetData<Data>();
            var barrel = Barrel.GetComponent<Barrel>(Context.World);

            Validate();
            PerformMutation();
            
            Context.ReleaseAll();
            
            var bulletFlightOperation = CreateBulletFlightOperation(barrel, data);
            yield return bulletFlightOperation.WaitEnumerator();
            OperationResult = bulletFlightOperation.GetResult<IHitController.IResult>();
        }
        
        private void Validate()
        {
            var magazine = Magazine.GetComponent<Magazine>(Context.World);
            if (magazine is not { Rounds: > 0 })
                throw new MagazineIsEmptyException();
        }

        private void PerformMutation()
        {
            var magazine = Magazine.GetComponent<Magazine>(Context.World);
            magazine.Rounds--;
            Magazine.SetComponent(magazine, Context.World);
        }

        private void RecordMutationUndo()
        {
            var magazine = Magazine.GetComponent<Magazine>(Context.World);
            var rounds = magazine.Rounds;

            Context.RecordUndo(() =>
            {
                Magazine.GetComponent<Magazine>(Context.World);
                magazine.Rounds = rounds;
                Magazine.SetComponent(magazine, Context.World);
            });
        }
        
        private BulletFlightOperation CreateBulletFlightOperation(Barrel barrel, Data data)
        {
            var executor = new OperationExecutor(Weapon);
            var from = barrel.Position;
            var direction = barrel.NormalizedForward;
            var distance = data.Distance;
            var operationData = new BulletFlightOperation.Data(from, direction, distance);
            var bulletFlightOperation = new BulletFlightOperation(Identifier, operationData, executor, Middlewares);
            bulletFlightOperation.RunOperation(Runner, Context.World);
            return bulletFlightOperation;
        }
        
        public readonly struct Data : IOperationData
        {
            public float Distance { get; }
            
            public Data(float distance)
            {
                Distance = distance;
            }
        }
    }
}