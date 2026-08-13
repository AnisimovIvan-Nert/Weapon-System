using System.Collections;
using System.Collections.Generic;
using OperationSystem.ComplexWeapons.Operations.Hit;
using OperationSystem.Operations;
using OperationSystem.Operations.Middleware;
using UnityEngine;

namespace OperationSystem.ComplexWeapons._Tests.Mocks
{
    public class RaycastHitCollectorMiddleware : AbstractOperationMiddleware<IRaycastOperationTag>
    {
        public List<RaycastHit> Hits { get; } = new();
        
        public override IEnumerator Complete(IOperation operation, IOperationContext context)
        {
            yield return base.Complete(operation, context);
            
            if (operation.OperationResult is not RaycastOperation.Result result)
                yield break;
            
            Hits.Add(result.Hit);
        }
    }
}