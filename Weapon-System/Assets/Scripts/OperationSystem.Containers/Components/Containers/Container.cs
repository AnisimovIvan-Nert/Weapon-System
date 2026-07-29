using OperationSystem.Component;

namespace OperationSystem.Containers.Components.Containers
{
    public interface IContainer : IComponent
    {
    }

    public class Container
        : AbstractComponent
        , IContainer
    {
    }
}