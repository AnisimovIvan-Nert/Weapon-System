using System.Collections;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Handlers.Units
{
    public abstract class AbstractUnitOperationHandler<T> 
        : AbstractOperationHandler
        , IUnitOperationHandler<T>
        where T : IUnit
    {
        public T? Unit { get; private set; }

        protected AbstractUnitOperationHandler(IOperationRunner operationRunner) 
            : base(operationRunner)
        {
        }

        public IEnumerator SetUnit(T? unit)
        {
            var delayer = OperationRunner.DelayOperationRunning();

            if (OperationRunner.AnyRunningOperation)
                yield return null;

            Unit = unit;
            OperationRunner.ReleaseOperationRunning(delayer);
        }
    }
}