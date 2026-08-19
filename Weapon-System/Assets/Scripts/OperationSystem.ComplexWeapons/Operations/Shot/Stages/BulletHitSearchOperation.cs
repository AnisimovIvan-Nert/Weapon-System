using System;
using System.Collections;
using Coroutine;
using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Operations.Hit;
using OperationSystem.ComplexWeapons.Operations.Hit.Result;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Result;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Shot.Stages
{
    public class BulletHitSearchOperation : AbstractOperation
    {
        public BulletHitSearchOperation(OperationIdentifier identifier, Data data, IOperationExecutor executor)
            : base(identifier, data, executor)
        {
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var data = this.GetData<Data>();
            var executor = this.GetData<IOperationExecutor>();
            
            RaycastHit raycastHit = default;
            yield return SendRaycast(data)
                .GetResult<RaycastHit>(result => raycastHit = result);
            
            yield return HandleHit(data, raycastHit, executor)
                .GetResult<IHitResult>(SetResult);
        }
        
        private IEnumerator SendRaycast(Data data)
        {
            var raycastData = new RaycastOperation.Data(data.Command);
            var raycastOperation = new RaycastOperation(Identifier, raycastData);
            RunOperation(raycastOperation);
            yield return raycastOperation.WaitEnumerator();
            var result = raycastOperation.GetResult<RaycastOperation.Result>();
            
            var hit = result.Hit;
            if (hit.collider?.GetComponentInParent<IAsset>() == null)
            {
                SetResult(new NotHitResult());
                throw new OperationForcedComplete();
            }

            yield return hit;
        }

        private IEnumerator HandleHit(Data data, RaycastHit raycastHit, IOperationExecutor executor)
        {
            var asset = raycastHit.collider.GetComponentInParent<IAsset>();
            var operationData = new HitOperation.Data(raycastHit, data.Command);
            var operationAsset = new OperationAsset(asset);
            var hitOperation = new HitOperation(Identifier, operationData, operationAsset, executor);
            RunOperation(hitOperation);
            yield return hitOperation.WaitEnumerator();
            var hitResult = hitOperation.GetResult<IHitResult>();

            switch (hitResult)
            {
                case NotHitResult notHitResult:
                    SetResult(notHitResult);
                    throw new OperationForcedComplete();
                case HitOperation.Result result:
                    yield return result.HandleOperation;
                    yield return result.HandleOperation.GetResult<IHitResult>();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        
        public readonly struct Data : IOperationData
        {
            public RaycastCommand Command { get; }
            
            public Data(RaycastCommand command)
            {
                Command = command;
            }
        }
    }
}