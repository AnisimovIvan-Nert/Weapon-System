using System.Collections.Generic;

namespace Weapons.Assets
{
    public interface IAsset
    {
        List<IAsset> Children { get; }

        IUnit ToUnit();
    }
}