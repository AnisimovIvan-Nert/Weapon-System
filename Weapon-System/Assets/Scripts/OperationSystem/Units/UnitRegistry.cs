using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OperationSystem.Assets;
using OperationSystem.Handlers;

namespace OperationSystem.Units
{
    public class UnitRegistry
    {
        private struct Slot
        {
            public bool Alive;
            public IAsset Asset;
            public Unit Unit;
        }

        private readonly object _lock = new();
        private readonly ConcurrentStack<int> _freeSlots;
        
        private UnitWorld _world;
        private Slot[] _slots;
        private int _count;

        public UnitRegistry(int initialCapacity = 1024)
        {
            _slots = new Slot[initialCapacity];
            _freeSlots = new ConcurrentStack<int>();
        }

        public void SetWorld(UnitWorld world) => _world = world;

        public Unit Create(IAsset asset, IOperationHandler handler)
        {
            var mask = asset.GetComponentMask();
            
            if (!_freeSlots.TryPop(out var id))
            {
                id = Interlocked.Increment(ref _count);
                if (_count > _slots.Length)
                    GrowSize(_count);
            }
            
            _slots[id] = new Slot
            {
                Alive = true,
                Asset = asset
            };

            var unitId = new UnitId(id);
            var children = asset.Children.Select(o => Create(o, handler));
            var unit = new Unit(unitId, mask, _world, handler, children.ToArray());
            
            _slots[id].Unit = unit;
            handler.AppendChild(unit);
            
            return unit;
        }

        public void Destroy(UnitId unitId)
        {
            var id = unitId.Id;
            if (!IsAlive(unitId))
                return;

            _slots[id].Alive = false;
            _freeSlots.Push(id);
        }

        public void Destroy(in Unit unit) => Destroy(unit.Id);

        public void Destroy(IEnumerable<Unit> units)
        {
            Exception? exception = null;
            foreach (var unit in units)
            {
                try
                {
                    Destroy(unit);
                }
                catch (Exception e)
                {
                    exception = exception == null
                        ? new AggregateException(e)
                        : new AggregateException(exception, e);
                }
            }

            if (exception != null)
                throw exception;
        }

        public bool IsAlive(UnitId unitId)
        {
            var id = unitId.Id;
            return id >= 0 && id < _count && _slots[id].Alive;
        }
        
        public IAsset GetAsset(UnitId unitId) => _slots[unitId.Id].Asset;
        public Unit GetUnit(IAsset asset) => _slots.First(slot => slot.Alive && slot.Asset == asset).Unit;

        public IEnumerable<UnitId> EnumerateAlive()
        {
            for (var i = 0; i < _count; i++)
                if (_slots[i].Alive)
                    yield return new UnitId(i);
        }

        private void GrowSize(int requireSize)
        {
            lock (_lock)
            {
                if (_slots.Length >= requireSize)
                    return;
                
                var newSize = _slots.Length * 2;
                Array.Resize(ref _slots, newSize);
            }
        }
    }
}