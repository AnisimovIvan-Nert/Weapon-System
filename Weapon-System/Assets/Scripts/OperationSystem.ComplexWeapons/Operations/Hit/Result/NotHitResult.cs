using OperationSystem.Assets;

namespace OperationSystem.ComplexWeapons.Operations.Hit.Result
{
    public readonly struct NotHitResult : IHitResult
    {
        public IAsset Source { get; }
        
        public NotHitResult(IAsset source)
        {
            Source = source;
        }
    }
}