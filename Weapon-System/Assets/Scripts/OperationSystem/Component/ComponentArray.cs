using System;
using ECS.Units;
using OperationSystem.Component;
using OperationSystem.Units;

namespace ECS
{
    public class ComponentArray<T> : IComponentArray 
        where T : struct, IComponent
    {
        private readonly object _lock;
        private readonly DirtyTracker _assetDirty;
        private readonly DirtyTracker _componentDirty;
        
        private T[] _components;

        public ComponentArray(int initialCapacity = 64)
        {
            _components = new T[initialCapacity];
            _assetDirty = new DirtyTracker(initialCapacity);
            _componentDirty = new DirtyTracker(initialCapacity);
            _lock = new object();
        }
        
        public ref T Get(UnitId unitId)
        {
            lock (_lock)
            {
                _componentDirty.SetDirty(unitId);
                return ref ReadOnly(unitId);
            }
        }
        
        public ref T ReadOnly(UnitId unitId)
        {
            lock (_lock)
            {
                EnsureIndexInRange(unitId.Id);
                return ref _components[unitId.Id];
            }
        }
        
        public void SetAssetDirty(UnitId unitId)
        {
            lock (_lock)
            {
                EnsureIndexInRange(unitId.Id);
                _assetDirty.SetDirty(unitId);
            }
        }
        
        public void OnEntityDestroyed(UnitId unitId)
        {
            lock (_lock)
            {
                var id = unitId.Id;
                _assetDirty.Clear(unitId);
                _componentDirty.Clear(unitId);
                if (id < _components.Length)
                    _components[id] = default;
            }
        }

        public void PullFromAssets(UnitRegistry registry)
        {
            lock (_lock)
            {
                foreach (var unitId in _assetDirty)
                {
                    if (!registry.IsAlive(unitId))
                        continue;
                    if (registry.GetAsset(unitId) is IAssetPull<T> pull)
                        pull.PullInto(ref _components[unitId.Id]);
                }

                _assetDirty.ClearAll();
            }
        }

        public void PushToAssets(UnitRegistry registry)
        {
            lock (_lock)
            {
                foreach (var unitId in _componentDirty)
                {
                    if (!registry.IsAlive(unitId))
                        continue;
                    if (registry.GetAsset(unitId) is IAssetPush<T> push)
                        push.PushFrom(in _components[unitId.Id]);
                }

                _componentDirty.ClearAll();
            }
        }
        
        private void EnsureIndexInRange(int index)
        {
            lock (_lock)
            {
                if (index < _components.Length)
                    return;

                var newCapacity = Math.Max(index + 1, _components.Length * 2);
                Array.Resize(ref _components, newCapacity);
                _assetDirty.EnsureCapacity(newCapacity);
                _componentDirty.EnsureCapacity(newCapacity);
            }
        }
    }
}