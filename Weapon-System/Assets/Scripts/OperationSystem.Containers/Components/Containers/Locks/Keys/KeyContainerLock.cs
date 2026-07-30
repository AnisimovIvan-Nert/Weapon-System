using System;
using System.Collections;
using System.Linq;
using OperationSystem.Component;
using OperationSystem.Units;

namespace OperationSystem.Containers.Components.Containers.Locks.Keys
{
    public readonly struct KeyContainerLock : IContainerLock
    {
        private readonly Guid _identifier;

        public KeyContainerLock(Guid identifier) 
        {
            _identifier = identifier;
        }

        public IEnumerator CanInteractWithContainer(Unit unit)
        {
            var word = unit.World;
            var keysStorage = word.GetComponents<KeysStorage>().TryGetComponent(unit);

            if (keysStorage == null)
            {
                yield return false;
                yield break;
            }

            var identifier = _identifier;
            yield return keysStorage.Value.Keys.OfType<Key>().Any(key => key.Identifier == identifier);
        }
    }
}