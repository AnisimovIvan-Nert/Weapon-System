using OperationSystem.Operations.Result;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons.Operations.Hit.Result
{
    public interface IHitResult : IOperationResult
    {
        Unit Source { get; }
    }
}