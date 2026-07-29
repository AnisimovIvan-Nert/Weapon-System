using System;
using System.Collections.Generic;

namespace ECS.Units
{
    public class UnitRegistry
    {
        private struct Slot
        {
            public bool Alive;
            public int AssetHandle;
            public ComponentMask Mask;
            public int AssetTypeId;
        }

        private Slot[] _slots;
        private readonly Stack<int> _freeSlots;

        public int Capacity => _slots.Length;
        public int Count { get; private set; }

        public UnitRegistry(int initialCapacity = 1024)
        {
            _slots = new Slot[initialCapacity];
            _freeSlots = new Stack<int>(64);
        }

        public Unit Create(int assetHandle, int assetTypeId, ComponentMask mask)
        {
            int id;
            if (_freeSlots.Count > 0)
            {
                id = _freeSlots.Pop();
            }
            else
            {
                id = Count;
                Count++;
                if (Count > _slots.Length)
                    GrowSize();
            }

            _slots[id] = new Slot
            {
                Alive = true,
                AssetHandle = assetHandle,
                Mask = mask,
                AssetTypeId = assetTypeId
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
            return id >= 0 && id < Count && _slots[id].Alive;
        }

        public int GetAssetHandle(UnitId unitId) => _slots[unitId.Id].AssetHandle;
        public int GetAssetTypeId(UnitId unitId) => _slots[unitId.Id].AssetTypeId;
        public ComponentMask GetMask(UnitId unitId) => _slots[unitId.Id].Mask;

        public bool HasMask(UnitId unitId, ComponentMask mask)
        {
            return (_slots[unitId.Id].Mask & mask) == mask;
        }

        public IEnumerable<UnitId> AllAlive()
        {
            for (var i = 0; i < Count; i++)
                if (_slots[i].Alive)
                    yield return new UnitId(i);
        }

        private void GrowSize()
        {
            var newSize = _slots.Length * 2;
            Array.Resize(ref _slots, newSize);
        }
    }
}