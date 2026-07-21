using System.Collections.Generic;
using Weapons.Units;

namespace Weapons.Assets
{
    public interface IAsset
    {
        List<IAsset> Children { get; }

        IUnit ToUnit();
    }
}