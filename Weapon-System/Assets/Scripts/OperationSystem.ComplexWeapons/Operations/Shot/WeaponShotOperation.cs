using System.Collections;
using Coroutine;
using OperationSystem.ComplexWeapons.Operations.Shot.Stages;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Result;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Shot
{
    public class WeaponShotOperation : AbstractOperation
    {
        public WeaponShotOperation(OperationIdentifier identifier, Data data, IOperationAsset operationAsset)
            : base(identifier, operationAsset, data)
        {
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();

            var data = this.GetData<Data>();
            var operationAsset = this.GetData<IOperationAsset>();

            yield return TakeBullet(operationAsset);

            SpawnBulletOnBarrelOperation.Result spawnResult = default;
            yield return SpawnBullet(operationAsset)
                .GetResult<SpawnBulletOnBarrelOperation.Result>(result => spawnResult = result);

            IOperation operation = null!;
            yield return RunBulletFlightOperation(data, operationAsset, spawnResult)
                .GetResult<IOperation>(result => operation = result);

            SetResult(new Result(operation));
        }

        private IEnumerator TakeBullet(IOperationAsset operationAsset)
        {
            var takeBulletOperation = new TakeBulletFromMagazineOperation(Identifier, operationAsset);
            RunOperation(takeBulletOperation);
            yield return takeBulletOperation.WaitEnumerator();
            if (takeBulletOperation.Exception != null)
                throw takeBulletOperation.Exception;
        }

        private IEnumerator SpawnBullet(IOperationAsset operationAsset)
        {
            var spawnBulletOperation = new SpawnBulletOnBarrelOperation(Identifier, operationAsset);
            RunOperation(spawnBulletOperation);
            yield return spawnBulletOperation.WaitEnumerator();
            yield return spawnBulletOperation.GetResult<SpawnBulletOnBarrelOperation.Result>();
        }

        private IEnumerator RunBulletFlightOperation(
            Data data,
            IOperationAsset operationAsset,
            SpawnBulletOnBarrelOperation.Result spawnResult)
        {
            var executor = new OperationExecutor(operationAsset.Asset);
            var from = spawnResult.Position;
            var direction = spawnResult.Direction;
            var distance = data.Distance;
            var command = new RaycastCommand(from, direction, QueryParameters.Default, distance);
            var operationData = new BulletFlightOperation.Data(command);
            var bulletFlightOperation = new BulletFlightOperation(Identifier, operationData, executor);
            RunOperation(bulletFlightOperation);
            yield return bulletFlightOperation;
        }

        public readonly struct Data : IOperationData
        {
            public float Distance { get; }

            public Data(float distance)
            {
                Distance = distance;
            }
        }

        public readonly struct Result : IOperationResult
        {
            public IOperation Operation { get; }

            public Result(IOperation operation)
            {
                Operation = operation;
            }
        }
    }
}