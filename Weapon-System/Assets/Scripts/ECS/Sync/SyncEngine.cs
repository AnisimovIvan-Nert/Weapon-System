using System;
using System.Collections.Generic;
using ECS.Units;

namespace ECS
{
    public class SyncEngine
    {
        private readonly UnitRegistry _registry;
        private readonly ISyncAssetResolver _assetResolver;
        private readonly List<IComponentArray> _componentArrays;
        private IComponentArray[] _arrayByTypeId;

        public UnitRegistry Registry => _registry;
        public ISyncAssetResolver AssetResolver => _assetResolver;
        public IReadOnlyList<IComponentArray> ComponentArrays => _componentArrays;

        public SyncEngine(UnitRegistry registry, ISyncAssetResolver assetResolver)
        {
            _registry = registry;
            _assetResolver = assetResolver;
            _componentArrays = new List<IComponentArray>();
            _arrayByTypeId = Array.Empty<IComponentArray>();
        }

        private IComponentArray GetArrayByTypeId(int typeId)
        {
            if (typeId < _arrayByTypeId.Length)
                return _arrayByTypeId[typeId];
            return null;
        }

        private void SetArrayByTypeId(int typeId, IComponentArray array)
        {
            if (typeId >= _arrayByTypeId.Length)
                Array.Resize(ref _arrayByTypeId, Math.Max(typeId + 1, _arrayByTypeId.Length * 2));
            _arrayByTypeId[typeId] = array;
        }

        public ComponentArray<T> RegisterComponentType<T>() where T : struct, IComponent
        {
            var typeId = ComponentType<T>.Id;
            var existing = GetArrayByTypeId(typeId);
            if (existing != null)
                return (ComponentArray<T>)existing;

            var array = new ComponentArray<T>();
            _componentArrays.Add(array);
            SetArrayByTypeId(typeId, array);
            return array;
        }

        public ComponentArray<T> GetArray<T>() where T : struct, IComponent
        {
            return (ComponentArray<T>)_arrayByTypeId[ComponentType<T>.Id];
        }

        public ref T Get<T>(UnitId unitId) where T : struct, IComponent
        {
            return ref GetArray<T>().Get(unitId);
        }

        public ref T Get<T>(in Unit unit) where T : struct, IComponent
        {
            return ref GetArray<T>().Get(unit.Id);
        }

        public Unit CreateEntity(int assetHandle, int assetTypeId, ComponentMask mask)
        {
            var entity = _registry.Create(assetHandle, assetTypeId, mask);

            foreach (var typeId in mask)
            {
                var arr = GetArrayByTypeId(typeId);
                arr?.SetAssetDirty(entity.Id);
            }

            return entity;
        }

        public void DestroyEntity(in Unit unit)
        {
            var mask = _registry.GetMask(unit.Id);
            _registry.Destroy(unit);

            foreach (var typeId in mask)
            {
                var arr = GetArrayByTypeId(typeId);
                arr?.OnEntityDestroyed(unit.Id);
            }
        }

        public void MarkAssetTypeDirty(int assetTypeId)
        {
            foreach (var unitId in _registry.AllAlive())
            {
                if (_registry.GetAssetTypeId(unitId) != assetTypeId) continue;
                foreach (var typeId in _registry.GetMask(unitId))
                {
                    var arr = GetArrayByTypeId(typeId);
                    arr?.SetAssetDirty(unitId);
                }
            }
        }

        public void MarkSingleAssetDirty(int assetHandle)
        {
            foreach (var unitId in _registry.AllAlive())
            {
                if (_registry.GetAssetHandle(unitId) != assetHandle) continue;
                foreach (var typeId in _registry.GetMask(unitId))
                {
                    var arr = GetArrayByTypeId(typeId);
                    arr?.SetAssetDirty(unitId);
                }
            }
        }

        public void Pull()
        {
            foreach (var array in _componentArrays)
                array.PullFromAssets(_registry, _assetResolver);
        }

        public void Push()
        {
            foreach (var array in _componentArrays)
                array.PushToAssets(_registry, _assetResolver);
        }

        public void Update()
        {
            Pull();
            Push();
        }

        public void Update(Action systems)
        {
            Pull();
            systems();
            Push();
        }
    }
}
