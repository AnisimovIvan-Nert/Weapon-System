using OperationSystem.Component;
using OperationSystem.Operations;
using OperationSystem.Units;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Components.Handlers.Hit
{
    public interface IHitHandler 
    {
        IOperation CreateHandleOperation(IOperation source, RaycastHit hit, RaycastCommand command, Unit target);
    }
    
    public interface IHitHandlerComponent : IComponent
    {
        IHitHandler HitHandler { get; }
    }
    
    public readonly struct HitHandlerComponent : IHitHandlerComponent
    {
        public IHitHandler HitHandler { get; }
        
        public HitHandlerComponent(IHitHandler hitHandler)
        {
            HitHandler = hitHandler;
        }
        
        
    }
}