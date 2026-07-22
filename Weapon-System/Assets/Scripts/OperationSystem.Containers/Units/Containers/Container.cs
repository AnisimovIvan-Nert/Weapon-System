using OperationSystem.Units;

namespace OperationSystem.Containers.Units.Containers
{
    public interface IContainer : IUnit
    {
    }

    public class Container
        : AbstractUnit
        , IContainer
    {
        public Container(params IUnit[] children) 
            : base(children)
        {
        }
    }
}