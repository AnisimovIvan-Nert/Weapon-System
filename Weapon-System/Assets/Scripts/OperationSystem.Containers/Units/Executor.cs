using OperationSystem.Units;

namespace OperationSystem.Containers.Units
{
    public interface IExecutor : IUnit
    {
    }

    public class Executor
        : AbstractUnit
        , IExecutor
    {
        public Executor(params IUnit[] children) 
            : base(children)
        {
        }
    }
}