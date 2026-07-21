using System.Collections.Generic;
using Weapons.Resource;

namespace Weapons.Units
{
    public interface IUnit : IResource
    {
        IEnumerable<IUnit> Children { get; }
    }
}