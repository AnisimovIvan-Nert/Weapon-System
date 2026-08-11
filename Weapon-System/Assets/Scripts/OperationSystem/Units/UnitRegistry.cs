using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using OperationSystem.Assets;

namespace OperationSystem.Units
{
    public class UnitRegistry
    {
        private readonly ConcurrentDictionary<IAsset, Unit> _assetToUnit = new();
        private readonly ConcurrentStack<UnitId> _freeIds = new();
        private int _count;
        
        public Unit GetOrCreate(IAsset asset) => _assetToUnit.GetOrAdd(asset, CreateUnit);

        public bool Destroy(IAsset asset, out Unit unit)
        {
            if (!_assetToUnit.TryRemove(asset, out unit))
                return false;
            
            _freeIds.Push(unit.Id.CreateNewVersion());
            return true;
        }
        
        private Unit CreateUnit(IAsset asset)
        {
            var unitId = GetNewId();
            var componentMas = asset.GetComponentMask();
            var children = asset.Children.Select(GetOrCreate);
            return new Unit(unitId, componentMas, asset, children.ToArray());
        }

        private UnitId GetNewId()
        {
            if (!_freeIds.TryPop(out var unitId))
            {
                var id = Interlocked.Increment(ref _count);
                unitId = UnitId.Create(id);
            }

            return unitId;
        }
    }
}