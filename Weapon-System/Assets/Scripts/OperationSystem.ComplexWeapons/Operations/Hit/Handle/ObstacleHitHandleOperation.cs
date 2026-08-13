using System.Collections;
using OperationSystem.ComplexWeapons.Operations.Hit.Result;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Hit.Handle
{
    public class ObstacleHitHandleOperation : AbstractOperation
    {
        public ObstacleHitHandleOperation(
            OperationIdentifier identifier,
            Data data,
            IOperationExecutor operationExecutor) 
            : base(identifier, data, operationExecutor)
        {
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();

            var data = this.GetData<Data>();
            var executor = this.GetData<IOperationExecutor>().Executor;
            
            var position = data.Hit.point;
            var direction =  Vector3.Reflect(data.Command.direction, data.Hit.normal);
            var distance = data.Command.distance - data.Hit.distance;
            var command = new RaycastCommand(position, direction, data.Command.queryParameters, distance);

            var result = new RedirectHitResult(command, executor);
            SetResult(result);
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