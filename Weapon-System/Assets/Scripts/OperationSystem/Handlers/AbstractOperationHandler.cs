using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Handlers
{
    public abstract class AbstractOperationHandler : IOperationHandler
    {
        protected UnitWorld UnitWorld { get; }
        
        public IOperationRunner OperationRunner { get; }

        protected AbstractOperationHandler(IOperationRunner operationRunner, UnitWorld unitWorld)
        {
            OperationRunner = operationRunner;
            UnitWorld = unitWorld;
        }
        
        public virtual void Update()
        {
            OperationRunner.Update();
        }

        public virtual IOperationContext CreateContext() => new OperationContext(UnitWorld);
    }
}