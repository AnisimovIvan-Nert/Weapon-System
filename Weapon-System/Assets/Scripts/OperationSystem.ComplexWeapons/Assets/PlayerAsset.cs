using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Operations.Hit.Handle;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Assets
{
    public class PlayerAsset : AbstractAsset, IHitHandler
    {
        public IOperation CreateHandleOperation(IOperation source, RaycastHit raycastHit, RaycastCommand command, IAsset target)
        {
            var operationAsset = new OperationAsset(target);
            var executor = source.GetData<IOperationExecutor>();
            return new PlayerHitHandleOperation(source.Identifier, operationAsset, executor);
        }
    }
}