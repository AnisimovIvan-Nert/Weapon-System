using System;
using System.Collections;
using System.Linq;
using OperationSystem.Units;

namespace OperationSystem.Containers.Units.Containers.Locks.Keys
{
    public class KeyContainerLock 
        : AbstractUnit
        , IContainerLock
    {
        private readonly Guid _identifier;

        public KeyContainerLock(Guid identifier) 
            : base(Enumerable.Empty<IUnit>())
        {
            _identifier = identifier;
        }

        public IEnumerator CanInteractWithContainer(IUnit unit)
        {
            var keysStorage = unit.TryFind<IKeysStorage>();

            if (keysStorage == null)
            {
                yield return false;
                yield break;
            }

            yield return keysStorage.Keys.OfType<Key>().Any(key => key.Identifier == _identifier);
        }
    }
}