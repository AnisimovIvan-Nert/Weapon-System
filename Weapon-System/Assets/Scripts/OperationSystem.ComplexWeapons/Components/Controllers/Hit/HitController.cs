using System.Collections;
using OperationSystem.Component;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Result;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Components.Controllers.Hit
{
    public interface IHitController
    {
        IEnumerator HandleHit(Unit target, IOperationExecutor executor);
        
        public interface IResult : IOperationResult
        {
        }
    }
    
    public interface IHitControllerComponent : IComponent
    {
        IHitController HitController { get; }
    }
    
    public readonly struct HitControllerComponent : IHitControllerComponent
    {
        public IHitController HitController { get; }
        
        public HitControllerComponent(IHitController hitController)
        {
            HitController = hitController;
        }
    }
}