using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OperationSystem.Assets;

namespace OperationSystem.Units
{
    public class UnitRegistry
    {
        private readonly object _unitsLock = new();
        private readonly List<Unit> _units = new();
        private readonly ConcurrentDictionary<IAsset, Unit> _assetToUnit = new();
        private readonly ConcurrentStack<UnitId> _freeIds = new();
        private int _nextId;
        
        public Unit GetOrCreate(IAsset asset, UnitWorld world)
        {
            return _assetToUnit.GetOrAdd(asset, o => CreateUnit(o, world));
        }

        public bool Destroy(IAsset asset, out Unit unit)
        {
            if (!_assetToUnit.TryRemove(asset, out unit))
                return false;

            lock (_unitsLock)
                _units.Remove(unit);
            
            _freeIds.Push(unit.Id.CreateNewVersion());
            return true;
        }

        public IEnumerable<Unit> EnumerateUnits()
        {
            lock (_unitsLock)
                return new List<Unit>(_units);
        }
        
        private Unit CreateUnit(IAsset asset, UnitWorld world)
        {
            var unitId = GetNewId();
            var componentMas = asset.GetComponentMask();
            var children = asset.Children.Select(o => GetOrCreate(o, world));
            var unit = new Unit(unitId, componentMas, asset, children.ToArray());

            lock (_unitsLock)
                _units.Add(unit);
            
            world.OnUnitCreated(unit);
            return unit;
        }

        private UnitId GetNewId()
        {
            if (!_freeIds.TryPop(out var unitId))
            {
                var id = Interlocked.Increment(ref _nextId);
                unitId = UnitId.Create(id);
            }

            return unitId;
        }
    }
}