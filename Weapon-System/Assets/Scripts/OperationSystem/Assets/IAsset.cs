using System.Collections.Generic;
using OperationSystem.Units;

namespace OperationSystem.Assets
{
    public interface IAsset
    {
        List<IAsset> Children { get; }

        IEnumerable<IAsset> EnumerateUnitChildren();
        
        void CreateComponents(Unit unit, UnitWorld world);
        void BeforeUpdate(Unit unit, UnitWorld world);
        void AfterUpdate(Unit unit, UnitWorld world);
    }
}