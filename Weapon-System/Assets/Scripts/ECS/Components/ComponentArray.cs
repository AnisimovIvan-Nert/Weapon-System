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

        public ComponentArray(int maxEntities)
        {
            ComponentTypeId = ComponentType<T>.Id;
            _components = new T[Math.Max(1, maxEntities)];
            _assetDirty = new DirtyTracker(maxEntities);
            _componentDirty = new DirtyTracker(maxEntities);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(UnitId unitId)
        {
            _componentDirty.SetDirty(unitId);
            return ref _components[unitId.Id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadOnly(UnitId unitId)
        {
            return ref _components[unitId.Id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAssetDirty(UnitId unitId)
        {
            _assetDirty.SetDirty(unitId);
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
            _assetDirty.Clear(unitId);
            _componentDirty.Clear(unitId);
            _components[unitId.Id] = default;
        }

        public void Grow(int newCapacity)
        {
            Array.Resize(ref _components, newCapacity);
            _assetDirty.Resize(newCapacity);
            _componentDirty.Resize(newCapacity);
        }
    }
}
