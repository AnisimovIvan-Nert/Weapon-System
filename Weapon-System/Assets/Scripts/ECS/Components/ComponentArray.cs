using System;
using System.Runtime.CompilerServices;
using ECS.Units;

namespace ECS
{
    public class ComponentArray<T> : IComponentArray where T : struct, IComponent
    {
        private T[] _components;
        private DirtyTracker _assetDirty;
        private DirtyTracker _componentDirty;

        public int ComponentTypeId { get; }
        public Span<T> AllComponents => _components.AsSpan(0, _components.Length);

        public ComponentArray(int initialCapacity = 64)
        {
            ComponentTypeId = ComponentType<T>.Id;
            _components = new T[Math.Max(1, initialCapacity)];
            _assetDirty = new DirtyTracker(initialCapacity);
            _componentDirty = new DirtyTracker(initialCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(UnitId unitId)
        {
            EnsureCapacity(unitId.Id);
            _componentDirty.SetDirty(unitId);
            return ref _components[unitId.Id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadOnly(UnitId unitId)
        {
            EnsureCapacity(unitId.Id);
            return ref _components[unitId.Id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAssetDirty(UnitId unitId)
        {
            _assetDirty.SetDirty(unitId);
        }

        private void EnsureCapacity(int id)
        {
            if (id >= _components.Length)
            {
                var newLen = Math.Max(id + 1, _components.Length * 2);
                Array.Resize(ref _components, newLen);
                _assetDirty.EnsureCapacity(newLen - 1);
                _componentDirty.EnsureCapacity(newLen - 1);
            }
        }

        public void PullFromAssets(UnitRegistry registry, ISyncAssetResolver assetResolver)
        {
            foreach (var unitId in _assetDirty)
            {
                if (!registry.IsAlive(unitId)) continue;
                if (assetResolver.GetAsset(registry.GetAssetHandle(unitId)) is IAssetPull<T> pull)
                    pull.PullInto(ref _components[unitId.Id]);
            }

            _assetDirty.ClearAll();
        }

        public void PushToAssets(UnitRegistry registry, ISyncAssetResolver assetResolver)
        {
            foreach (var unitId in _componentDirty)
            {
                if (!registry.IsAlive(unitId)) continue;
                if (assetResolver.GetAsset(registry.GetAssetHandle(unitId)) is IAssetPush<T> push)
                    push.PushFrom(in _components[unitId.Id]);
            }

            _componentDirty.ClearAll();
        }

        public void OnEntityDestroyed(UnitId unitId)
        {
            var id = unitId.Id;
            _assetDirty.Clear(unitId);
            _componentDirty.Clear(unitId);
            if (id < _components.Length)
                _components[id] = default;
        }

        public void Grow(int newCapacity)
        {
            if (newCapacity <= _components.Length) return;
            Array.Resize(ref _components, newCapacity);
            _assetDirty.EnsureCapacity(newCapacity - 1);
            _componentDirty.EnsureCapacity(newCapacity - 1);
        }
    }
}