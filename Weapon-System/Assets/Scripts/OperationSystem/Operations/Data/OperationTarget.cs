using OperationSystem.Units;

namespace OperationSystem.Operations.Data
{
    public interface IOperationTarget : IOperationData
    {
        Unit Target { get; }
    }
    
    public readonly struct OperationTarget : IOperationTarget
    {
        public Unit Target { get; }
        
        public OperationTarget(Unit target)
        {
            Target = target;
        }
    }
}