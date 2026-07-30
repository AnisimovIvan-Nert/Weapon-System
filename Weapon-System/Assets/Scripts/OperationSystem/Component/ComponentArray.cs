using System;
using System.Collections.Concurrent;
using System.Threading;
using OperationSystem.Assets;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Component
{
    public class ComponentArray<T> : IComponentArray<T>
        where T : struct, IComponent
    {
        private struct Slot
        {
            public T Component;
            public OperationIdentifier Owner;
        }

        private readonly object _lock;
        private readonly DirtyTracker _assetDirty;
        private readonly DirtyTracker _componentDirty;
        private readonly ConcurrentStack<int> _freeSlots;
        private readonly ConcurrentDictionary<UnitId, int> _unitToSlot;

        private Slot[] _slots;
        private int _count;

        public ComponentArray(int initialCapacity = 64)
        {
            _slots = new Slot[initialCapacity];
            _freeSlots = new ConcurrentStack<int>();
            _unitToSlot = new ConcurrentDictionary<UnitId, int>();
            _assetDirty = new DirtyTracker(initialCapacity);
            _componentDirty = new DirtyTracker(initialCapacity);
            _lock = new object();
        }

        public bool HasComponent(Unit unit) => unit.ComponentMask.Contains<T>();

        public T GetComponent(UnitId unitId)
        {
            lock (_lock)
            {
                var index = GetOrAdd(unitId);
                return _slots[index].Component;
            }
        }
        
        public TComponent GetComponent<TComponent>(UnitId unitId) where TComponent : IComponent
        {
            var component = GetComponent(unitId);
            if (component is not TComponent typedComponent) 
                throw new InvalidOperationException();
            return typedComponent;
        }
        
        public void SetComponent(UnitId unitId, T component)
        {
            lock (_lock)
            {
                var index = GetOrAdd(unitId);
                _componentDirty.SetDirty(index);
                _slots[index].Component = component;
            }
        }
        
        public void SetComponent<TComponent>(UnitId unitId, TComponent component) where TComponent : IComponent
        {
            if (component is not T typedComponent) 
                throw new InvalidOperationException();
            SetComponent(unitId, typedComponent);
        }

        public OperationIdentifier GetOwner(UnitId unitId)
        {
            lock (_lock)
            {
                var index = GetOrAdd(unitId);
                return _slots[index].Owner;
            }
        }
        
        public void SetOwner(UnitId unitId, OperationIdentifier owner)
        {
            lock (_lock)
            {
                var index = GetOrAdd(unitId);
                _slots[index].Owner = owner;
            }
        }

        public void SetAssetDirty(UnitId unitId)
        {
            lock (_lock)
            {
                var index = GetOrAdd(unitId);
                _assetDirty.SetDirty(index);
            }
        }

        public void OnEntityDestroyed(UnitId unitId)
        {
            lock (_lock)
            {
                if (!_unitToSlot.TryRemove(unitId, out var index))
                    return;

                _assetDirty.Clear(index);
                _componentDirty.Clear(index);
                _slots[index] = default;
                _freeSlots.Push(index);
            }
        }

        public void PullFromAssets(UnitId unitId, UnitRegistry unitRegistry)
        {
            lock (_lock)
            {
                if (!_unitToSlot.TryGetValue(unitId, out var index))
                    return;

                if (unitRegistry.GetAsset(unitId) is IAssetPull<T> pull)
                    pull.PullInto(ref _slots[index].Component);
                
                _assetDirty.Clear(index);
            }
        }

        public void PushToAssets(UnitId unitId, UnitRegistry unitRegistry)
        {
            lock (_lock)
            {
                if (!_unitToSlot.TryGetValue(unitId, out var index))
                    return;

                if (unitRegistry.GetAsset(unitId) is IAssetPush<T> push)
                    push.PushFrom(in _slots[index].Component);
                
                _componentDirty.Clear(index);
            }
        }

        private int GetOrAdd(UnitId unitId)
        {
            return _unitToSlot.GetOrAdd(unitId, ValueFactory);

            int ValueFactory(UnitId id)
            {
                if (!_freeSlots.TryPop(out var slotIndex))
                {
                    slotIndex = Interlocked.Increment(ref _count);
                    if (_count > _slots.Length)
                        EnsureIndexInRange(_count);
                }

                _slots[slotIndex] = default;
                return slotIndex;
            }
        }

        private void EnsureIndexInRange(int index)
        {
            lock (_lock)
            {
                if (index < _slots.Length)
                    return;

                var newCapacity = Math.Max(index + 1, _slots.Length * 2);
                Array.Resize(ref _slots, newCapacity);
                _assetDirty.EnsureCapacity(newCapacity);
                _componentDirty.EnsureCapacity(newCapacity);
            }
        }
    }
}