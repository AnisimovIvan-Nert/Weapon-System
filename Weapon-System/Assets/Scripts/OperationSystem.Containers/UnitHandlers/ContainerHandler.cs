using OperationSystem.Containers.Units.Containers;
using OperationSystem.Handlers.Units;
using OperationSystem.Operations;

namespace OperationSystem.Containers.UnitHandlers
{
    public class ContainerHandler : AbstractUnitOperationHandler<IContainer>
    {
        public ContainerHandler(IOperationRunner operationRunner) 
            : base(operationRunner)
        {
        }
    }
}