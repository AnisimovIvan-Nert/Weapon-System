using Weapons.Operations;
using Weapons.Units;

namespace Weapons.UnitHandlers
{
    public class AbstractUnitHandler<T> : IUnitHandler<T>
        where T : IUnit
    {
        public T Unit { get; }
        public IOperationRunner OperationRunner { get; }

        public AbstractUnitHandler(T unit, IOperationRunner operationRunner)
        {
            Unit = unit;
            OperationRunner = operationRunner;
        }
        
        public void Update()
        {
            OperationRunner.Update();
        }
    }
}