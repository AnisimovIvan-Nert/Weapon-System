using System.Collections;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Result;
using OperationSystem.Units;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Shot.Stages
{
    public class SpawnBulletOnBarrelOperation : AbstractOperation
    {
        private Unit Weapon => this.GetData<IOperationUnit>().Unit;
        private Unit Barrel => Weapon.GetChild<Barrel>(Context.World);

        public SpawnBulletOnBarrelOperation(OperationIdentifier identifier, IOperationUnit operationUnit)
            : base(identifier, operationUnit)
        {
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();
            
            Context.Acquire<Barrel>(Barrel, Identifier);
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var barrel = Barrel.GetComponent<Barrel>(Context.World);
            SetResult(new Result(barrel.Position, barrel.NormalizedForward));
        }
        public readonly struct Result : IOperationResult
        {
            public Vector3 Position { get; }
            public Vector3 Direction { get; }
            
            public Result(Vector3 position, Vector3 direction)
            {
                Position = position;
                Direction = direction;
            }
        }
    }
}