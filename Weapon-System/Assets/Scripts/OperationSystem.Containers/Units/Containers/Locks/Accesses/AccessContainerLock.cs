using System.Collections;
using System.Linq;
using OperationSystem.Units;

namespace OperationSystem.Containers.Units.Containers.Locks.Accesses
{
    public class AccessContainerLock 
        : AbstractUnit
        , IContainerLock
    {
        private readonly int _level;
        
        public AccessContainerLock(int level) 
            : base(Enumerable.Empty<IUnit>())
        {
            _level = level;
        }

        public IEnumerator CanInteractWithContainer(IUnit unit)
        {
            var accessLevel = unit.TryFind<IAccessLevel>();
            
            if (accessLevel == null)
            {
                yield return false;
                yield break;
            }

            yield return accessLevel.Level >= _level;
        }
    }
}