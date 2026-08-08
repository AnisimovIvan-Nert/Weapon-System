using OperationSystem.Assets;
using OperationSystem.Containers.Components.Containers;
using OperationSystem.Handlers;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Containers.UnitHandlers
{
    public class ContainerHandler : AbstractOperationHandler
    {
        public ContainerHandler(IOperationRunner operationRunner, UnitWorld unitWorld) 
            : base(operationRunner, unitWorld)
        {
        }

        protected override bool IsValidAsset(IAsset asset)
        {
            return asset.GetComponentMask().Contains<Container>();
        }
    }
}