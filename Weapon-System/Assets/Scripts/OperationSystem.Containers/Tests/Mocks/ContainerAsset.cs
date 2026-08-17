using OperationSystem.Assets;
using OperationSystem.Component.Types;
using OperationSystem.Containers.Components.Containers;
using OperationSystem.Containers.Components.Containers.Locks.Accesses;
using OperationSystem.Containers.Components.Containers.Locks.Keys;
using OperationSystem.Units;

namespace OperationSystem.Containers.Tests.Mocks
{
    public class ContainerAsset
        : AbstractAsset
        , IAssetPull<KeyContainerLock>
        , IAssetPull<AccessContainerLock>
        , IAssetPull<ContainerVolume>
    {
        public KeyContainerLock? KeyLock { get; private set; }
        public AccessContainerLock? AccessLock { get; private set; }
        public ContainerVolume? Volume { get; private set; }

        public ContainerAsset(
            KeyContainerLock? keyLock = null,
            AccessContainerLock? accessLock = null,
            ContainerVolume? containerVolume = null)
        {
            KeyLock = keyLock;
            AccessLock = accessLock;
            Volume = containerVolume;
        }

        public void Set(
            KeyContainerLock? keyLock = null,
            AccessContainerLock? accessLock = null,
            ContainerVolume? containerVolume = null,
            params IAsset[] children)
        {
            KeyLock = keyLock;
            AccessLock = accessLock;
            Volume = containerVolume;
            foreach (var child in children)
                TryAddChild(child);
        }

        public override ComponentMask GetComponentMask()
        {
            var mask = base.GetComponentMask();
            mask.Add<Container>();

            if (KeyLock.HasValue)
                mask.Add<KeyContainerLock>();
            if (AccessLock.HasValue)
                mask.Add<AccessContainerLock>();
            if (Volume.HasValue)
                mask.Add<ContainerVolume>();

            return mask;
        }

        public void PullInto(ref KeyContainerLock component, UnitWorld world)
        {
            if (KeyLock.HasValue)
                component = KeyLock.Value;
        }

        public void PullInto(ref AccessContainerLock component, UnitWorld world)
        {
            if (AccessLock.HasValue)
                component = AccessLock.Value;
        }

        public void PullInto(ref ContainerVolume component, UnitWorld world)
        {
            if (Volume.HasValue)
                component = Volume.Value;
        }
    }
}