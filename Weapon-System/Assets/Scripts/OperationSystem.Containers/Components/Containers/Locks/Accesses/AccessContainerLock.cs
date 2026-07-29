using System.Collections;
using OperationSystem.Component;
using OperationSystem.Units;

namespace OperationSystem.Containers.Components.Containers.Locks.Accesses
{
    public class AccessContainerLock 
        : AbstractComponent
        , IContainerLock
    {
        private readonly int _level;
        
        public AccessContainerLock(int level) 
        {
            _level = level;
        }

        public IEnumerator CanInteractWithContainer(Unit unit)
        {
            var accessLevel = unit.ComponentsData.TryGet<IAccessLevel>();
            
            if (accessLevel == null)
            {
                yield return false;
                yield break;
            }

            yield return accessLevel.Level >= _level;
        }
    }
}