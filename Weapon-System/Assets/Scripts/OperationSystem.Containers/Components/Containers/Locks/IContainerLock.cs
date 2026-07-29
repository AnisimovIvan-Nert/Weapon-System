using System.Collections;
using OperationSystem.Component;
using OperationSystem.Units;

namespace OperationSystem.Containers.Components.Containers.Locks
{
    public interface IContainerLock : IComponent
    {
        IEnumerator CanInteractWithContainer(Unit unit);
    }
}