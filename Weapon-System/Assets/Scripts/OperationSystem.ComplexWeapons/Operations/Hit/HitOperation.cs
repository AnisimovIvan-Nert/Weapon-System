using System.Collections;
using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Assets;
using OperationSystem.ComplexWeapons.Operations.Hit.Result;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Hit
{
    public class HitOperation : AbstractOperation
    {
        private IAsset Asset => this.GetData<IOperationAsset>().Asset;
        
        public HitOperation(
            OperationIdentifier identifier,
            Data data,
            IOperationAsset operationAsset,
            IOperationExecutor executor) 
            : base(identifier, data, operationAsset, executor)
        {
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var executor = this.GetData<IOperationExecutor>();
            
            if (Asset is not IHitHandler hitHandler)
            {
                SetResult(new NotHitResult(executor.Executor));
                yield break;
            }

            var data = this.GetData<Data>();
            
            var operation = hitHandler.CreateHandleOperation(this, data.Hit, data.Command, Asset);
            RunOperation(operation);
            SetResult(new Result(operation, executor.Executor));
        }

        public readonly struct Result : IHitResult
        {
            public IAsset Source { get; }
            public IOperation HandleOperation { get; }
            
            public Result(IOperation operation, IAsset source)
            {
                HandleOperation = operation;
                Source = source;
            }
        }

        public readonly struct Data : IOperationData
        {
            public RaycastHit Hit { get; }
            public RaycastCommand Command { get; }
            
            public Data(RaycastHit hit, RaycastCommand command)
            {
                Hit = hit;
                Command = command;
            }
        }
    }
}