using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Operations.Hit.Handle;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Assets
{
    public class ObstacleAsset : AbstractAsset, IHitHandler
    {
        public IOperation CreateHandleOperation(IOperation source, RaycastHit raycastHit, RaycastCommand command, IAsset target)
        {
            var executor = source.GetData<IOperationExecutor>();
            var data = new ObstacleHitHandleOperation.Data(raycastHit, command);
            return new ObstacleHitHandleOperation(source.Identifier, data, executor);
        }
    }
}