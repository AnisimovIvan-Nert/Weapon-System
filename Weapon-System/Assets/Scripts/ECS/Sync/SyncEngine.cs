using System;
using System.Collections.Generic;
using ECS.Shared;
using ECS.Units;

namespace ECS
{
    public class SyncEngine
    {
        private readonly UnitRegistry _registry;
        private readonly ISyncAssetResolver _assetResolver;
        private readonly List<IComponentArray> _componentArrays;
        private readonly Dictionary<int, IComponentArray> _arrayByTypeId;
        private readonly IComponentArray[] _arrayByTypeIdIndex;

        private int _maxEntities;

        public UnitRegistry Registry => _registry;
        public ISyncAssetResolver AssetResolver => _assetResolver;
        public IReadOnlyList<IComponentArray> ComponentArrays => _componentArrays;

        public SyncEngine(UnitRegistry registry, ISyncAssetResolver assetResolver, int maxEntities = 4096)
        {
            _registry = registry;
            _assetResolver = assetResolver;
            _componentArrays = new List<IComponentArray>();
            _arrayByTypeId = new Dictionary<int, IComponentArray>();
            _arrayByTypeIdIndex = new IComponentArray[64];
            _maxEntities = maxEntities;
        }

        public ComponentArray<T> RegisterComponentType<T>() where T : IComponent
        {
            var typeId = ComponentType<T>.Id;
            if (_arrayByTypeIdIndex[typeId] != null)
                return (ComponentArray<T>)_arrayByTypeIdIndex[typeId];

            var array = new ComponentArray<T>(_maxEntities);
            _componentArrays.Add(array);
            _arrayByTypeId[typeId] = array;
            _arrayByTypeIdIndex[typeId] = array;
            return array;
        }

        public ComponentArray<T> GetArray<T>() where T : IComponent
        {
            return (ComponentArray<T>)_arrayByTypeIdIndex[ComponentType<T>.Id];
        }

        public ref T Get<T>(UnitId unitId) where T : IComponent
        {
            return ref GetArray<T>().Get(unitId);
        }

        public ref T Get<T>(in Unit unit) where T : IComponent
        {
            return ref GetArray<T>().Get(unit.Id);
        }

        public Unit CreateEntity(int assetHandle, int assetTypeId, ComponentMask mask)
        {
            var entity = _registry.Create(assetHandle, assetTypeId, mask);

            var typesToRegister = mask.RawValue;
            while (typesToRegister != 0)
            {
                var tz = BitOperations.TrailingZeroCount(typesToRegister);
                typesToRegister &= typesToRegister - 1;
                if (_arrayByTypeIdIndex[tz] != null)
                {
                    _arrayByTypeIdIndex[tz].SetAssetDirty(entity.Id);
                }
            }

            return entity;
        }

        public void DestroyEntity(in Unit unit)
        {
            var mask = _registry.GetMask(unit.Id).RawValue;
            _registry.Destroy(unit);

            while (mask != 0)
            {
                var tz = BitOperations.TrailingZeroCount(mask);
                mask &= mask - 1;
                _arrayByTypeIdIndex[tz]?.OnEntityDestroyed(unit.Id);
            }
        }

        public void MarkAssetTypeDirty(int assetTypeId)
        {
            foreach (var unitId in _registry.AllAlive())
            {
                if (_registry.GetAssetTypeId(unitId) == assetTypeId)
                {
                    var mask = _registry.GetMask(unitId).RawValue;
                    var remaining = mask;
                    while (remaining != 0)
                    {
                        var tz = BitOperations.TrailingZeroCount(remaining);
                        remaining &= remaining - 1;
                        _arrayByTypeIdIndex[tz]?.SetAssetDirty(unitId);
                    }
                }
            }
        }

        public void MarkSingleAssetDirty(int assetHandle)
        {
            foreach (var unitId in _registry.AllAlive())
            {
                if (_registry.GetAssetHandle(unitId) == assetHandle)
                {
                    var mask = _registry.GetMask(unitId).RawValue;
                    var remaining = mask;
                    while (remaining != 0)
                    {
                        var tz = BitOperations.TrailingZeroCount(remaining);
                        remaining &= remaining - 1;
                        _arrayByTypeIdIndex[tz]?.SetAssetDirty(unitId);
                    }
                }
            }
        }

        public void Pull()
        {
            foreach (var array in _componentArrays)
            {
                array.PullFromAssets(_registry, _assetResolver);
            }
        }

        public void Push()
        {
            foreach (var array in _componentArrays)
            {
                array.PushToAssets(_registry, _assetResolver);
            }
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
