using System.Collections.Generic;
using Weapons.User;

namespace Weapons.Assets
{
    public interface IAsset
    {
        List<IAsset> Children { get; }

        IUnit ToUnit(IUserAdapter userAdapter);
    }
}