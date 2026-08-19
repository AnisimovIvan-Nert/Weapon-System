using System.Collections;
using OperationSystem.Assets;
using OperationSystem.ComplexWeapons.Operations.Hit.Result;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;

namespace OperationSystem.ComplexWeapons.Operations.Hit.Handle
{
    public class PlayerHitHandleOperation : AbstractOperation
    {
        public PlayerHitHandleOperation(
            OperationIdentifier identifier,
            IOperationAsset operationAsset,
            IOperationExecutor operationExecutor) 
            : base(identifier, operationAsset, operationExecutor)
        {
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();

            var asset = this.GetData<IOperationAsset>().Asset;
            var executor = this.GetData<IOperationExecutor>().Executor;

            SetResult(new Result(asset, executor));
        }

        public readonly struct Result : IHitResult
        {
            public IAsset Target { get; }
            public IAsset Source { get; }
            
            public Result(IAsset target, IAsset source)
            {
                Target = target;
                Source = source;
            }
        }
    }
}