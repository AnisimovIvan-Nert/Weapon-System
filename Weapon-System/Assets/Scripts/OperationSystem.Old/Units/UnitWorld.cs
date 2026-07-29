using System.Collections.Generic;
using System.Linq;
using OperationSystem.Assets;
using OperationSystem.Component;

namespace OperationSystem.Units
{
    public readonly struct UnitWorld
    {
        private readonly Dictionary<UnitId, IAsset> _units;
        private readonly UnitId[] _nextId;
        private readonly object _lock;
        
        public UnitId NextId
        {
            get => _nextId[0];
            set => _nextId[0] = value;
        }
        
        private UnitWorld(bool _)
        {
            _units = new Dictionary<UnitId, IAsset>();
            _nextId = new[] { new UnitId() };
            _lock = new object();
        }

        public static UnitWorld Create() => new(true);

        public Unit CreateUnit(IAsset asset)
        {
            lock (_lock)
            {
                var componentsData = ComponentsData.Create();
                var unit = new Unit(NextId, componentsData);
            
                _units.Add(NextId, asset);

                while (_units.ContainsKey(NextId))
                    NextId = new UnitId(NextId.Id + 1);

                foreach (var handle in asset.EnumerateComponents(this))
                    componentsData.AddComponent(handle);

                return unit;
            }
        }

        public IAsset GetAsset(UnitId id)
        {
            lock (_lock)
                return _units[id];
        }

        public void RemoveUnits(params UnitId[] ids)
        {
            lock (_lock)
            {
                foreach (var id in ids)
                    _units.Remove(id);
            }
        }
    }
}