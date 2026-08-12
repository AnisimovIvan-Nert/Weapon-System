using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using OperationSystem.Assets;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations
{
    public class BulletFlightOperation : AbstractOperation
    {
        public BulletFlightOperation(
            OperationIdentifier identifier,
            Data data,
            IOperationExecutor executor,
            IOperationMiddleware[] middlewares)
            : base(identifier, middlewares, data, executor)
        {
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var data = this.GetData<Data>();
            var executor = this.GetData<IOperationExecutor>();

            var raycastCommand = new RaycastCommand(data.From, data.Direction, QueryParameters.Default, data.Distance);
            var raycastData = new RaycastOperation.Data(raycastCommand);
            var raycastOperation = new RaycastOperation(Identifier, raycastData, Middlewares);
            raycastOperation.RunOperation(Runner, Context.World);

            yield return raycastOperation.WaitEnumerator();
            
            if (raycastOperation.Exception != null)
                ExceptionDispatchInfo.Capture(raycastOperation.Exception).Throw();

            if (raycastOperation.OperationResult is not RaycastOperation.Result raycastResult)
                throw new InvalidOperationException();

            var asset = raycastResult.Hit.collider.GetComponentInParent<IAsset>();
            var unit = Context.World.GetOrCreateUnit(asset);
            
            var operationUnit = new OperationUnit(unit);
            var hitOperation = new HitOperation(Identifier, operationUnit, executor, Middlewares);
        }

        public readonly struct Data : IOperationData
        {
            public Vector3 From { get; }
            public Vector3 Direction { get; }
            public float Distance { get; }
            
            public Data(Vector3 from, Vector3 direction, float distance)
            {
                From = from;
                Direction = direction;
                Distance = distance;
            }
        }
    }
}