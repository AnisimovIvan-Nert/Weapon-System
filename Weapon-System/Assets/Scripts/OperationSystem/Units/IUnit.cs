using System.Collections.Generic;
using OperationSystem.Resource;

namespace OperationSystem.Units
{
    public interface IUnit : IResource
    {
        IEnumerable<IUnit> Children { get; }
    }
}