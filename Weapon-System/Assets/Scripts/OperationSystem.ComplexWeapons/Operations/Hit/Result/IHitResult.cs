using OperationSystem.Assets;
using OperationSystem.Operations.Result;

namespace OperationSystem.ComplexWeapons.Operations.Hit.Result
{
    public interface IHitResult : IOperationResult
    {
        IAsset Source { get; }
    }
}