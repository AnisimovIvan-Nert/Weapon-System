using OperationSystem.Units;

namespace OperationSystem.Containers.Tests.Mocks
{
    public class FooUnit : AbstractUnit
    {
        public FooUnit(params IUnit[] children) : base(children)
        {
        }
    }
}