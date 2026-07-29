using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using OperationSystem.Assets;
using OperationSystem.Units;

namespace ECS.Units
{
    public class UnitRegistry
    {
        private struct Slot
        {
            public bool Alive;
            public IAsset Asset;
            public ComponentMask Mask;
        }

        private readonly object _lock = new();
        private readonly ConcurrentStack<int> _freeSlots;

        private Slot[] _slots;
        private int _count;

        public int Capacity => _slots.Length;

        public UnitRegistry(int initialCapacity = 1024)
        {
            _slots = new Slot[initialCapacity];
            _freeSlots = new ConcurrentStack<int>();
        }

        public Unit Create(IAsset asset)
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
                Asset = asset,
                Mask = mask,
            };

            var unitId = new UnitId(id);
            return new Unit(unitId);
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

        public bool IsAlive(UnitId unitId)
        {
            var id = unitId.Id;
            return id >= 0 && id < _count && _slots[id].Alive;
        }
        
        public IAsset GetAsset(UnitId unitId) => _slots[unitId.Id].Asset;
        public ComponentMask GetMask(UnitId unitId) => _slots[unitId.Id].Mask;

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