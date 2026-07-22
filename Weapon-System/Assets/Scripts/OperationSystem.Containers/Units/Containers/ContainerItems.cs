using System.Collections.Generic;
using System.Linq;
using OperationSystem.Units;

namespace OperationSystem.Containers.Units.Containers
{
    public interface IContainerItems : IUnit
    {
        IList<IUnit> Items { get; }
    }

    public class ContainerItems
        : AbstractUnit
        , IContainerItems
    {
        public IList<IUnit> Items { get; }

        public override IEnumerable<IUnit> Children => Items;

        public ContainerItems(params IUnit[] children) 
            : base(Enumerable.Empty<IUnit>())
        {
            Items = children.ToList();
        }
    }
}