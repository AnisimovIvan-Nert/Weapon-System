using System.Collections.Generic;

namespace Weapons
{
    public interface IUnit
    {
        IEnumerable<IUnit> Children { get; }
    }
}