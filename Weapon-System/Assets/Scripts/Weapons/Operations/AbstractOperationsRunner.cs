using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Weapons.Operations
{
    public abstract class AbstractOperationsRunner<T> : IOperationsRunner
        where T : IUnit
    {
        protected readonly List<IOperation<T>> Operations = new();

        public void Update(IUnit unit)
        {
            if (unit is not T typedUnit)
                return;

            HandleEvents(typedUnit);
            HandleOperations(typedUnit);
        }

        protected virtual void HandleOperations(T unit)
        {
            for (var index = 0; index < Operations.Count; index++)
            {
                var operation = Operations[index];
                if (operation.Increment(unit))
                    continue;

                Operations.RemoveAt(index);
                index--;

                HandleResult(unit, operation);
            }
        }

        protected abstract void HandleEvents(T unit);

        protected virtual void HandleResult(T unit, IOperation<T> operation)
        {
            if (operation.Exception != null)
                ExceptionDispatchInfo.Capture(operation.Exception).Throw();
        }
    }
}