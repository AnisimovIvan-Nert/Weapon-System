using OperationSystem.Handlers.Units;
using OperationSystem.Operations;

namespace OperationSystem.Containers.UnitHandlers
{
    public class ContainerHandler : AbstractUnitOperationHandler
    {
        public ContainerHandler(IOperationRunner operationRunner) 
            : base(operationRunner)
        {
        }
    }
}