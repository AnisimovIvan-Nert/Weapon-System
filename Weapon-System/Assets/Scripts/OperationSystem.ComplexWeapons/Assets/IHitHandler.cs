using OperationSystem.Assets;
using OperationSystem.Operations;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Assets
{
    public interface IHitHandler 
    {
        IOperation CreateHandleOperation(IOperation source, RaycastHit hit, RaycastCommand command, IAsset target);
    }
}