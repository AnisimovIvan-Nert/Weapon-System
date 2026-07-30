using OperationSystem.Assets;
using OperationSystem.Component.Types;
using OperationSystem.Containers.Components.Containers;
using OperationSystem.Containers.Components.Containers.Locks.Accesses;
using OperationSystem.Containers.Components.Containers.Locks.Keys;

namespace OperationSystem.Containers.Tests.Mocks
{
    public class ContainerAsset
        : AbstractAsset
        , IAssetSync<ContainerItems>
        , IAssetPull<KeyContainerLock>
        , IAssetPull<AccessContainerLock>
        , IAssetPull<ContainerVolume>
    {
        private readonly KeyContainerLock? _keyLock;
        private readonly AccessContainerLock? _accessLock;
        private readonly ContainerVolume? _volume;
        
        public ContainerItems Items { get; private set; }

        public ContainerAsset(
            ContainerItems items, 
            KeyContainerLock? keyLock = null, 
            AccessContainerLock? accessLock = null,
            ContainerVolume? containerVolume = null)
        {
            Items = items;
            _keyLock = keyLock;
            _accessLock = accessLock;
        }

        public override ComponentMask GetComponentMask()
        {
            var mask = ComponentMask.Create<Container>();
            mask.Add<ContainerItems>();
            
            if (_keyLock.HasValue)
                mask.Add<KeyContainerLock>();
            if (_accessLock.HasValue)
                mask.Add<AccessContainerLock>();
            if (_volume.HasValue)
                mask.Add<ContainerVolume>();

            return mask;
        }
        
        public void PullInto(ref ContainerItems component)
        {
            component = Items;
        }

        public void PushFrom(in ContainerItems component)
        {
            Items = component;
        }

        public void PullInto(ref KeyContainerLock component)
        {
            if (_keyLock.HasValue)
                component = _keyLock.Value;
        }

        public void PullInto(ref AccessContainerLock component)
        {
            if (_accessLock.HasValue)
                component = _accessLock.Value;
        }
        
        public void PullInto(ref ContainerVolume component)
        {
            if (_volume.HasValue)
                component = _volume.Value;
        }
    }
}