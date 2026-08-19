using System.Collections;
using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Assets;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Result;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Shot.Stages
{
    public class SpawnBulletOnBarrelOperation : AbstractOperation
    {
        private IAsset Weapon => this.GetData<IOperationAsset>().Asset;
        private BarrelAsset Barrel => Weapon.GetChild<BarrelAsset>();

        public SpawnBulletOnBarrelOperation(OperationIdentifier identifier, IOperationAsset operationAsset)
            : base(identifier, operationAsset)
        {
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();
            
            Context.Acquire(Barrel, Identifier);
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();

            var barrel = Barrel;
            SetResult(new Result(barrel.transform.position, barrel.transform.forward.normalized));
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