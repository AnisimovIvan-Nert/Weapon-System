using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Operations.Hit.Result
{
    public readonly struct NotHitResult : IHitResult
    {
        public Unit Source { get; }
        
        public NotHitResult(Unit source)
        {
            Source = source;
        }
    }
}