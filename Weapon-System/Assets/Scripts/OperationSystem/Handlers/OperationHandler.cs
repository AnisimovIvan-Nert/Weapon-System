using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Handlers
{
    public class OperationHandler : AbstractOperationHandler
    {
        public OperationHandler(IOperationRunner operationRunner, UnitWorld unitWorld) 
            : base(operationRunner, unitWorld)
        {
        }
    }
}