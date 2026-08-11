using System;
using System.Collections.Concurrent;
using OperationSystem.Assets;
using OperationSystem.Component.Types;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Component
{
    public class ComponentArray<T> : IComponentArray<T>
        where T : struct, IComponent
    {
        private const int InitialCapacity = 64;

        private readonly DirtyTracker _componentDirty = new(InitialCapacity);
        private readonly ConcurrentDictionary<int, Slot> _unitIdToSlot = new();

        public int TypeId => ComponentType<T>.Id;

        public bool HasComponent(Unit unit) => unit.ComponentMask.Contains<T>();

        public T GetComponent(UnitId unitId) => GetOrAddSlot(unitId).Component;

        public TComponent GetComponent<TComponent>(UnitId unitId) where TComponent : IComponent
        {
            var component = GetComponent(unitId);
            if (component is not TComponent typedComponent)
                throw new InvalidOperationException();
            return typedComponent;
        }

        public void SetComponent(UnitId unitId, T component)
        {
            var slot = GetOrAddSlot(unitId);
            _componentDirty.SetDirty(unitId.Id);
            slot.Component = component;
        }

        public void SetComponent<TComponent>(UnitId unitId, TComponent component) where TComponent : IComponent
        {
            if (component is not T typedComponent)
                throw new InvalidOperationException();
            SetComponent(unitId, typedComponent);
        }

        public void DestroyComponent(UnitId unitId)
        {
            if (!TryRemoveSlot(unitId))
                return;

            var index = unitId.Id;

            _componentDirty.Clear(index);
        }

        public bool TryAcquireComponent(UnitId unitId, OperationIdentifier owner)
        {
            var slot = GetOrAddSlot(unitId);

            lock (slot.OwnerLock)
            {
                if (slot.Owner == owner)
                    return true;

                if (slot.Owner != default)
                    return false;

                slot.Owner = owner;
                return true;
            }
        }

        public void ReleaseComponent(UnitId unitId, OperationIdentifier owner)
        {
            var slot = GetOrAddSlot(unitId);

            lock (slot.OwnerLock)
            {
                if (slot.Owner != owner)
                    throw new InvalidOperationException();

                slot.Owner = default;
            }
        }

        public void PullFromAssets(Unit unit)
        {
            if (unit.Asset is not IAssetPull<T> pull)
                return;

            var slot = GetOrAddSlot(unit.Id);
            var component = slot.Component;
            pull.PullInto(ref component);
            slot.Component = component;
        }

        public void PushToAssets(Unit unit)
        {
            if (unit.Asset is not IAssetPush<T> push)
                return;

            var slot = GetOrAddSlot(unit.Id);
            var index = slot.Id.Id;

            if (!_componentDirty.IsDirty(index))
                return;

            push.PushFrom(slot.Component);
            _componentDirty.Clear(index);
        }

        private Slot GetOrAddSlot(UnitId unitId)
        {
            var slot = _unitIdToSlot.GetOrAdd(unitId.Id, ValueFactory);

            if (slot.Id.Version != unitId.Version)
                throw new UnitDestroyedException(unitId);

            return slot;

            Slot ValueFactory(int id) => new(unitId);
        }

        private bool TryRemoveSlot(UnitId unitId)
        {
            if (!_unitIdToSlot.TryGetValue(unitId.Id, out var slot))
                return false;

            if (slot.Id.Version != unitId.Version)
                throw new UnitDestroyedException(unitId);

            return _unitIdToSlot.TryRemove(unitId.Id, out _);
        }

        private class Slot
        {
            public UnitId Id { get; }
            public T Component { get; set; }
            public OperationIdentifier Owner { get; set; }
            public object OwnerLock { get; }

            public Slot(UnitId id)
            {
                Id = id;
                Component = default;
                Owner = default;
                OwnerLock = new object();
            }
        }
    }
}