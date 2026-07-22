using System.Collections;
using OperationSystem.Units;

namespace OperationSystem.Containers.Units.Containers.Locks
{
    public interface IContainerLock : IUnit
    {
        IEnumerator CanInteractWithContainer(IUnit unit);
    }
}