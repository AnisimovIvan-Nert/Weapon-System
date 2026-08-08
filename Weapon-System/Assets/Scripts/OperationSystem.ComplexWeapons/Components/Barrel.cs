using OperationSystem.Component;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Components
{
    public interface IBarrel : IComponent
    {
        Vector3 NormalizedForward { get; }
    }
    
    public readonly struct Barrel : IBarrel
    {
        public Vector3 NormalizedForward { get; }
        
        public Barrel(Vector3 normalizedForward)
        {
            NormalizedForward = normalizedForward;
        }
    }
}