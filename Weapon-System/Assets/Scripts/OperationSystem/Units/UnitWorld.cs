using System.Collections.Generic;
using System.Linq;
using OperationSystem.Assets;
using OperationSystem.Component;

namespace OperationSystem.Units
{
    public readonly struct UnitWorld
    {
        private readonly Dictionary<UnitId, Unit> _idToUnitMap;
        private readonly Dictionary<IAsset, UnitId> _objectToIdMap;
        private readonly UnitId[] _nextId;
        
        public IReadOnlyDictionary<UnitId, Unit> Units => _idToUnitMap;
        
        public UnitId NextId
        {
            get => _nextId[0];
            set => _nextId[0] = value;
        }
        
        private UnitWorld(bool _)
        {
            _idToUnitMap = new Dictionary<UnitId, Unit>();
            _objectToIdMap = new Dictionary<IAsset, UnitId>();

            _nextId = new[] { new UnitId() };
        }

        public static UnitWorld Create() => new(true);

        public void Clear()
        {
            _idToUnitMap.Clear();
            _objectToIdMap.Clear();
        }

        public Unit GetOrAddUnit(IAsset asset)
        {
            if (_objectToIdMap.TryGetValue(asset, out var id))
               return _idToUnitMap[id];

            var data = ComponentsData.Create();
            var unit = new Unit(NextId, data);
            
            _objectToIdMap.Add(asset, NextId);
            _idToUnitMap.Add(NextId, unit);

            while (_idToUnitMap.ContainsKey(NextId))
                NextId = new UnitId(NextId.Id + 1);

            var children = asset.Children.Select(GetOrAddUnit).Select(o => o.Id);
            var childrenComponent = new ChildrenComponent(children.ToArray());
            data.AddComponent(childrenComponent);
            
            asset.CreateComponents(unit, this);

            return unit;
        }
    }
}