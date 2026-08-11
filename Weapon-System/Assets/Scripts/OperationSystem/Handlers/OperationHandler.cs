using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Handlers
{
    public class OperationHandler : AbstractOperationHandler
    {
        public OperationHandler(UnitWorld world, IOperationRunner? runner = null) 
            : base(world, runner)
        {
        }
    }
}