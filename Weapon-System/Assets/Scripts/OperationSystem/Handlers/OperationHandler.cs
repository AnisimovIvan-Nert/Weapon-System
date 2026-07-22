using OperationSystem.Operations;

namespace OperationSystem.Handlers
{
    public class OperationHandler : AbstractOperationHandler
    {
        public OperationHandler(IOperationRunner operationRunner) 
            : base(operationRunner)
        {
        }
    }
}