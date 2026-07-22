using System.Collections;
using Weapons.Operations;
using Weapons.Units;

namespace Weapons.UnitHandlers
{
    public class AbstractUnitHandler<T> : IUnitHandler<T>
        where T : IUnit
    {
        public T? Unit { get; private set; }
        public IOperationRunner OperationRunner { get; }

        public AbstractUnitHandler(IOperationRunner operationRunner)
        {
            OperationRunner = operationRunner;
        }
        
        public void Update()
        {
            OperationRunner.Update();
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