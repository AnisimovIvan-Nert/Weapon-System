using System.Collections;
using OperationSystem.Component;
using OperationSystem.Units;

namespace OperationSystem.Containers.Components.Containers.Locks.Accesses
{
    public readonly struct AccessContainerLock : IContainerLock
    {
        private readonly int _level;

        public AccessContainerLock(int level)
        {
            _level = level;
        }

        public IEnumerator CanInteractWithContainer(Unit unit)
        {
            var word = unit.World;
            var accessLevel = word.GetComponents<AccessLevel>().TryGetComponent(unit);

            if (accessLevel == null)
            {
                yield return false;
                yield break;
            }

            yield return accessLevel.Value.Level >= _level;
        }
    }
}