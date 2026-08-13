using OperationSystem.Component;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Components
{
    public interface IBarrel : IComponent
    {
        Vector3 NormalizedForward { get; }
        Vector3 Position { get; }
    }
    
    public readonly struct Barrel : IBarrel
    {
        public Vector3 NormalizedForward { get; }
        public Vector3 Position { get; }
        
        public Barrel(Vector3 normalizedForward, Vector3 position)
        {
            NormalizedForward = normalizedForward;
            Position = position;
        }
    }
}