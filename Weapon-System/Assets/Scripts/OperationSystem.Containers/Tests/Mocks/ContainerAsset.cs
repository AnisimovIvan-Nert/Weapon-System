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
        public KeyContainerLock? KeyLock { get; }
        public AccessContainerLock? AccessLock { get; }
        public ContainerVolume? Volume { get; }
        
        public ContainerItems Items { get; private set; }

        public ContainerAsset(
            ContainerItems items, 
            KeyContainerLock? keyLock = null, 
            AccessContainerLock? accessLock = null,
            ContainerVolume? containerVolume = null)
        {
            Items = items;
            KeyLock = keyLock;
            AccessLock = accessLock;
            Volume = containerVolume;
        }

        public override ComponentMask GetComponentMask()
        {
            var mask = ComponentMask.Create<Container>();
            mask.Add<ContainerItems>();
            
            if (KeyLock.HasValue)
                mask.Add<KeyContainerLock>();
            if (AccessLock.HasValue)
                mask.Add<AccessContainerLock>();
            if (Volume.HasValue)
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
            if (KeyLock.HasValue)
                component = KeyLock.Value;
        }

        public void PullInto(ref AccessContainerLock component)
        {
            if (AccessLock.HasValue)
                component = AccessLock.Value;
        }
        
        public void PullInto(ref ContainerVolume component)
        {
            if (Volume.HasValue)
                component = Volume.Value;
        }
    }
}