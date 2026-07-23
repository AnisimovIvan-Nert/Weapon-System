using OperationSystem.Units;

namespace OperationSystem.Operations.Data
{
    public interface IOperationTarget : IOperationData
    {
        IUnit Target { get; }
    }
    
    public class OperationTarget : IOperationTarget
    {
        public IUnit Target { get; }
        
        public OperationTarget(IUnit target)
        {
            Target = target;
        }
    }
}