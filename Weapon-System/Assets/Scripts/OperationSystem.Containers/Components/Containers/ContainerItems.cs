using System.Collections.Generic;
using System.Linq;
using OperationSystem.Component;
using OperationSystem.Units;

namespace OperationSystem.Containers.Components.Containers
{
    public interface IContainerItems : IComponent
    {
        IList<Unit> Items { get; }
    }

    public struct ContainerItems : IContainerItems
    {
        public IList<Unit> Items { get; }

        private ContainerItems(IList<Unit> item)
        {
            Items = item;
        }

        public static ContainerItems Create(params Unit[] children)
        {
            return new ContainerItems(children.ToList());
        }
    }
}