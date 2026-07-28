using System.Collections.Generic;
using OperationSystem.Units;

namespace OperationSystem.Assets
{
    public interface IAsset
    {
        List<IAsset> Children { get; }

        void CreateComponents(Unit unit, UnitWorld world);
        void ReadComponents(Unit unit, UnitWorld world);
        void SetComponents(Unit unit, UnitWorld world);
    }
}