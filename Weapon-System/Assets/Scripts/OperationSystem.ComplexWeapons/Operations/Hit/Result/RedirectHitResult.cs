using OperationSystem.Units;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Hit.Result
{
    public readonly struct RedirectHitResult : IHitResult
    {
        public RaycastCommand Command { get; }
        public Unit Source { get; }
        
        public RedirectHitResult(RaycastCommand command, Unit source)
        {
            Command = command;
            Source = source;
        }
    }
}