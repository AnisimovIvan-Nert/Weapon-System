using OperationSystem.ComplexWeapons.Operations.Hit.Handle;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Units;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Components.Handlers.Hit
{
    public class PlayerHitHandler : IHitHandler
    {
        public IOperation CreateHandleOperation(IOperation source, RaycastHit raycastHit, RaycastCommand command, Unit target)
        {
            var operationUnit = new OperationUnit(target);
            var executor = source.GetData<IOperationExecutor>();
            return new PlayerHitHandleOperation(source.Identifier, operationUnit, executor);
        }
    }
}