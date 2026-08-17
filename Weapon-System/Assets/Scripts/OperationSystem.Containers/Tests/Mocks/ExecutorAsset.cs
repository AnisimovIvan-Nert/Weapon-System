using OperationSystem.Assets;
using OperationSystem.Component.Types;
using OperationSystem.Containers.Components;
using OperationSystem.Units;

namespace OperationSystem.Containers.Tests.Mocks
{
    public class ExecutorAsset 
        : AbstractAsset
        , IAssetPull<KeysStorage>
        , IAssetPull<AccessLevel>
    {
        private KeysStorage? _keysStorage;
        private AccessLevel? _accessLevel;

        public ExecutorAsset(KeysStorage? keysStorage = null, AccessLevel? accessLevel = null)
        {
            _keysStorage = keysStorage;
            _accessLevel = accessLevel;
        }

        public void Set(KeysStorage? keysStorage = null, AccessLevel? accessLevel = null)
        {
            _keysStorage = keysStorage;
            _accessLevel = accessLevel;
        }

        public override ComponentMask GetComponentMask()
        {
            var mask = base.GetComponentMask();
            mask.Add<Executor>();
            
            if (_keysStorage != null)
                mask.Add<KeysStorage>();
            if (_accessLevel != null)
                mask.Add<AccessLevel>();

            return mask;
        }
        
        public void PullInto(ref KeysStorage component, UnitWorld world)
        {
            if (_keysStorage.HasValue)
                component = _keysStorage.Value;
        }

        public void PullInto(ref AccessLevel component, UnitWorld world)
        {
            if (_accessLevel.HasValue)
                component = _accessLevel.Value;
        }
    }
}