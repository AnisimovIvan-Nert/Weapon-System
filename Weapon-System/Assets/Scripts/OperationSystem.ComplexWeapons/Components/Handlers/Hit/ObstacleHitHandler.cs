using OperationSystem.ComplexWeapons.Operations.Hit.Handle;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Units;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Components.Handlers.Hit
{
    public class ObstacleHitHandler : IHitHandler
    {
        public IOperation CreateHandleOperation(IOperation source, RaycastHit raycastHit, RaycastCommand command, Unit target)
        {
            var executor = source.GetData<IOperationExecutor>();
            var data = new ObstacleHitHandleOperation.Data(raycastHit, command);
            return new ObstacleHitHandleOperation(source.Identifier, data, executor);
        }
    }
}