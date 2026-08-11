using System.Collections;
using OperationSystem.Units;

namespace OperationSystem.Containers.Components.Containers.Locks.Accesses
{
    public readonly struct AccessContainerLock : IContainerLock
    {
        public int Level { get; }

        public AccessContainerLock(int level)
        {
            Level = level;
        }

        public IEnumerator CanInteractWithContainer(Unit unit, UnitWorld world)
        {
            if (!unit.TryGetComponent<AccessLevel>(world, out var accessLevel))
            {
                yield return false;
                yield break;
            }

            yield return accessLevel.Level >= Level;
        }
    }
}