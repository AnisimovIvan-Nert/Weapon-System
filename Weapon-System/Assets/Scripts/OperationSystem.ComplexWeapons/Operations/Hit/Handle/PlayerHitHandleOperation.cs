using System.Collections;
using OperationSystem.ComplexWeapons.Operations.Hit.Result;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Operations.Hit.Handle
{
    public class PlayerHitHandleOperation : AbstractOperation
    {
        public PlayerHitHandleOperation(
            OperationIdentifier identifier,
            IOperationUnit operationUnit,
            IOperationExecutor operationExecutor) 
            : base(identifier, operationUnit, operationExecutor)
        {
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();

            var unit = this.GetData<IOperationUnit>().Unit;
            var executor = this.GetData<IOperationExecutor>().Executor;

            SetResult(new Result(unit, executor));
        }

        public readonly struct Result : IHitResult
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