using OperationSystem.Assets;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Hit.Result
{
    public readonly struct RedirectHitResult : IHitResult
    {
        public RaycastCommand Command { get; }
        public IAsset Source { get; }
        
        public RedirectHitResult(RaycastCommand command, IAsset source)
        {
            Command = command;
            Source = source;
        }
    }
}