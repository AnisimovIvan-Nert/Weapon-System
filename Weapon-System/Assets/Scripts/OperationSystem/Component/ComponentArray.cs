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

        public T GetComponent(Unit unit) => GetOrAddSlot(unit).Component;

        public TComponent GetComponent<TComponent>(Unit unit) where TComponent : IComponent
        {
            var component = GetComponent(unit);
            if (component is not TComponent typedComponent)
                throw new InvalidOperationException();
            return typedComponent;
        }

        public void SetComponent(Unit unit, T component)
        {
            var slot = GetOrAddSlot(unit);
            _componentDirty.SetDirty(unit.Id.Id);
            slot.Component = component;
        }

        public void SetComponent<TComponent>(Unit unit, TComponent component) where TComponent : IComponent
        {
            if (component is not T typedComponent)
                throw new InvalidOperationException();
            SetComponent(unit, typedComponent);
        }

        public void DestroyComponent(Unit unit)
        {
            if (!TryRemoveSlot(unit))
                return;

            var index = unit.Id.Id;

            _componentDirty.Clear(index);
        }

        public bool TryAcquireComponent(Unit unit, OperationIdentifier owner)
        {
            var slot = GetOrAddSlot(unit);

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

        public void ReleaseComponent(Unit unit, OperationIdentifier owner)
        {
            var slot = GetOrAddSlot(unit);

            lock (slot.OwnerLock)
            {
                if (slot.Owner != owner)
                    throw new InvalidOperationException();

                slot.Owner = default;
            }
        }

        public void PullFromAsset(Unit unit, UnitWorld world)
        {
            if (unit.Asset is not IAssetPull<T> pull)
                return;
            
            if (!TryAcquireComponent(unit, world.WorldIdentifier))
                return;
            
            var slot = GetOrAddSlot(unit);
            var component = slot.Component;

            try
            {
                pull.PullInto(ref component, world);
            }
            finally
            {
                ReleaseComponent(unit, world.WorldIdentifier);
            }
            
            slot.Component = component;
        }

        public void PushToAsset(Unit unit, UnitWorld world)
        {
            if (unit.Asset is not IAssetPush<T> push)
                return;

            var slot = GetOrAddSlot(unit);
            var index = slot.Id.Id;

            if (!_componentDirty.IsDirty(index))
                return;

            if (!TryAcquireComponent(unit, world.WorldIdentifier))
                return;

            try
            {
                push.PushFrom(slot.Component, world);
            }
            finally
            {
                ReleaseComponent(unit, world.WorldIdentifier);
            }
            
            _componentDirty.Clear(index);
        }

        private Slot GetOrAddSlot(Unit unit)
        {
            var slot = _unitIdToSlot.GetOrAdd(unit.Id.Id, ValueFactory);

            if (slot.Id.Version != unit.Id.Version)
                throw new UnitDestroyedException(unit.Id);

            return slot;

            Slot ValueFactory(int id)
            {
                return unit.ComponentMask.Contains(TypeId) 
                    ? new Slot(unit.Id) 
                    : throw new InvalidOperationException();
            }
        }

        private bool TryRemoveSlot(Unit unit)
        {
            if (!_unitIdToSlot.TryGetValue(unit.Id.Id, out var slot))
                return false;

            if (slot.Id.Version != unit.Id.Version)
                throw new UnitDestroyedException(unit.Id);

            return _unitIdToSlot.TryRemove(unit.Id.Id, out _);
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