using System;
using System.Collections;
using System.Linq;
using OperationSystem.Units;

namespace OperationSystem.Containers.Components.Containers.Locks.Keys
{
    public readonly struct KeyContainerLock : IContainerLock
    {
        public Guid Identifier { get; }

        public KeyContainerLock(Guid identifier) 
        {
            Identifier = identifier;
        }

        public IEnumerator CanInteractWithContainer(Unit unit, UnitWorld world)
        {
            if (!unit.TryGetComponent<KeysStorage>(world, out var keysStorage))
            {
                yield return false;
                yield break;
            }
            
            var identifier = Identifier;
            yield return keysStorage.Keys.OfType<Key>().Any(key => key.Identifier == identifier);
        }
    }
}