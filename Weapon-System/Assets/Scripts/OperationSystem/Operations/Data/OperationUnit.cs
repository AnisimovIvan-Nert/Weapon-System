using OperationSystem.Units;

namespace OperationSystem.Operations.Data
{
    public interface IOperationUnit : IOperationData
    {
        Unit Unit { get; }
    }
    
    public readonly struct OperationUnit : IOperationUnit
    {
        public Unit Unit { get; }
        
        public OperationUnit(Unit unit)
        {
            Unit = unit;
        }
    }
}