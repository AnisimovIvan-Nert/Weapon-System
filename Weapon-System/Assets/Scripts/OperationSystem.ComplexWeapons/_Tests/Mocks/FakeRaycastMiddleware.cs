using System.Collections;
using OperationSystem.ComplexWeapons.Operations.Hit;
using OperationSystem.Operations;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Result;
using UnityEngine;

namespace OperationSystem.ComplexWeapons._Tests.Mocks
{
    public class FakeRaycastMiddleware : AbstractOperationMiddleware<IRaycastOperationTag>
    {
        private readonly RaycastHit _fakeHit;
        private int _repeat;

        public FakeRaycastMiddleware(RaycastHit fakeHit, int repeat = 1)
        {
            _fakeHit = fakeHit;
            _repeat = repeat;
        }

        public override IEnumerator Execute(IOperation operation, IOperationContext context)
        {
            yield return base.Execute(operation, context);
            
            if (_repeat-- <= 0)
                yield break;
            
            operation.SetResult(new RaycastOperation.Result(_fakeHit));
            throw new OperationForcedComplete();
        }
    }
}