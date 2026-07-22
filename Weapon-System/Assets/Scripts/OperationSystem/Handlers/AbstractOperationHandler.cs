using OperationSystem.Operations;

namespace OperationSystem.Handlers
{
    public abstract class AbstractOperationHandler : IOperationHandler
    {
        public IOperationRunner OperationRunner { get; }

        protected AbstractOperationHandler(IOperationRunner operationRunner)
        {
            OperationRunner = operationRunner;
        }
        
        public void Update()
        {
            OperationRunner.Update();
        }
    }
}