using System;
using System.Collections;
using System.Linq;
using OperationSystem.Component;
using OperationSystem.Units;

namespace OperationSystem.Containers.Components.Containers.Locks.Keys
{
    public class KeyContainerLock 
        : AbstractComponent
        , IContainerLock
    {
        private readonly Guid _identifier;

        public KeyContainerLock(Guid identifier) 
        {
            _identifier = identifier;
        }

        public IEnumerator CanInteractWithContainer(Unit unit)
        {
            var keysStorage = unit.ComponentsData.TryGet<IKeysStorage>();

            if (keysStorage == null)
            {
                yield return false;
                yield break;
            }

            yield return keysStorage.Keys.OfType<Key>().Any(key => key.Identifier == _identifier);
        }
    }
}