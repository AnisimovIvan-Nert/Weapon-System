using System.Collections;
using OperationSystem.ComplexWeapons.Components.Handlers.Hit;
using OperationSystem.ComplexWeapons.Operations.Hit.Result;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Units;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Hit
{
    public class HitOperation : AbstractOperation
    {
        private Unit Unit => this.GetData<IOperationUnit>().Unit;
        
        public HitOperation(
            OperationIdentifier identifier,
            Data data,
            IOperationUnit operationUnit,
            IOperationExecutor executor) 
            : base(identifier, data, operationUnit, executor)
        {
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var executor = this.GetData<IOperationExecutor>();
            
            if (!Unit.HasComponent<HitHandlerComponent>(Context.World))
            {
                SetResult(new NotHitResult(executor.Executor));
                yield break;
            }

            var data = this.GetData<Data>();
            
            var handler = Unit.GetComponent<HitHandlerComponent>(Context.World);
            var operation = handler.HitHandler.CreateHandleOperation(this, data.Hit, data.Command, Unit);
            RunOperation(operation);
            SetResult(new Result(operation, executor.Executor));
        }

        public readonly struct Result : IHitResult
        {
            public Unit Source { get; }
            public IOperation HandleOperation { get; }
            
            public Result(IOperation operation, Unit source)
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