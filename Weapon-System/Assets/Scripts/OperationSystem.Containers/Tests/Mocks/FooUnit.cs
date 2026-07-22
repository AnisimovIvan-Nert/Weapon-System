using System.Linq;
using OperationSystem.Units;

namespace OperationSystem.Containers.Tests.Mocks
{
    public class FooUnit : AbstractUnit
    {
        public FooUnit() : base(Enumerable.Empty<IUnit>())
        {
        }
    }
}