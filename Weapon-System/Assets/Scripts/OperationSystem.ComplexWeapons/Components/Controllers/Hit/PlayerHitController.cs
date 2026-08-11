using System.Collections;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Result;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Components.Controllers.Hit
{
    public class PlayerHitController : IHitController
    {
        public IEnumerator HandleHit(Unit target, IOperationExecutor executor)
        {
            yield return new Result(target, executor.Executor);
        }
        
        public readonly struct Result : IHitController.IResult
        {
            public Unit Target { get; }
            public Unit Source { get; }
            
            public Result(Unit target, Unit source)
            {
                Target = target;
                Source = source;
            }
        }
    }
}