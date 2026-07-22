using System.Collections.Generic;
using OperationSystem.Units;

namespace OperationSystem.Assets
{
    public interface IAsset
    {
        List<IAsset> Children { get; }

        IUnit ToUnit();
    }
}