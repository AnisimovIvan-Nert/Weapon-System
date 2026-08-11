using System.Collections;
using Coroutine;
using OperationSystem.ComplexWeapons.Components.Controllers.Hit;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Operations
{
    public class HitOperation : AbstractOperation
    {
        private Unit Unit => this.GetData<IOperationUnit>().Unit;
        
        public HitOperation(
            OperationIdentifier identifier,
            IOperationUnit operationUnit,
            IOperationExecutor executor,
            IOperationMiddleware[] middlewares) 
            : base(identifier, middlewares, operationUnit, executor)
        {
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            if (!Unit.HasComponent<HitControllerComponent>(Context.World))
                yield break;

            var executor = this.GetData<IOperationExecutor>();
            var controller = Unit.GetComponent<HitControllerComponent>(Context.World);
            
            yield return controller.HitController.HandleHit(Unit, executor)
                .GetResult<IHitController.IResult>(SetResult);
            
            yield break;

            void SetResult(IHitController.IResult result)
            {
                OperationResult = result;
            }
        }
    }
}