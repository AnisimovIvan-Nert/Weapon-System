using OperationSystem.Handlers.Units;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Containers.UnitHandlers
{
    public class ContainerHandler : AbstractUnitOperationHandler
    {
        public ContainerHandler(IOperationRunner operationRunner, UnitWorld unitWorld) 
            : base(operationRunner, unitWorld)
        {
        }
    }
}