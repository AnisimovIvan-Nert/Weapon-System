using System.Collections.Generic;
using ECS.Units;
using UnityEngine;

namespace ECS.Examples
{
    public class SyncManager : MonoBehaviour, ISyncAssetResolver
    {
        [SerializeField] private PlayerAsset[] _players;
        [SerializeField] private int _maxEntities = 4096;

        private UnitRegistry _registry;
        private SyncEngine _engine;
        private Dictionary<int, IAsset> _assetByHandle;
        private Dictionary<int, int> _assetTypeByHandle;

        private static int _nextHandle;
        private static readonly int _playerTypeId = 0;

        private void Awake()
        {
            _registry = new UnitRegistry(_maxEntities);
            _engine = new SyncEngine(_registry, this, _maxEntities);
            _assetByHandle = new Dictionary<int, IAsset>();
            _assetTypeByHandle = new Dictionary<int, int>();

            _engine.RegisterComponentType<TransformComponent>();
            _engine.RegisterComponentType<HealthComponent>();
            _engine.RegisterComponentType<InventoryComponent>();
            _engine.RegisterComponentType<ProjectileComponent>();
            _engine.RegisterComponentType<WeaponComponent>();
        }

        private void Start()
        {
            var playerMask = ComponentMask.FromTypes<TransformComponent, HealthComponent, InventoryComponent>();

            foreach (var player in _players)
            {
                var handle = _nextHandle++;
                _assetByHandle[handle] = player;
                _assetTypeByHandle[handle] = _playerTypeId;

                _engine.CreateEntity(handle, _playerTypeId, playerMask);
            }
        }

        private void Update()
        {
            _engine.Update(RunSystems);
        }

        private void RunSystems()
        {
            var transforms = _engine.GetArray<TransformComponent>();
            var healths = _engine.GetArray<HealthComponent>();

            foreach (var unitId in _registry.AllAlive())
            {
                if (!_registry.HasMask(unitId, healthMask)) continue;

                ref var health = ref healths.Get(unitId);
                if (health.Current <= 0f)
                {
                    var entity = new Unit(unitId);
                    _engine.DestroyEntity(entity);
                }
            }
        }

        private static readonly ComponentMask healthMask = ComponentMask.FromTypes<HealthComponent>();

        public IAsset GetAsset(int assetHandle)
        {
            _assetByHandle.TryGetValue(assetHandle, out var asset);
            return asset;
        }

        public int GetAssetTypeId(int assetHandle)
        {
            _assetTypeByHandle.TryGetValue(assetHandle, out var typeId);
            return typeId;
        }
    }
}
