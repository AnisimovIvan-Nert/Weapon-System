using OperationSystem.Component;

namespace OperationSystem.Containers.Components
{
    public interface IExecutor : IComponent
    {
    }

    public class Executor
        : AbstractComponent
        , IExecutor
    {
    }
}