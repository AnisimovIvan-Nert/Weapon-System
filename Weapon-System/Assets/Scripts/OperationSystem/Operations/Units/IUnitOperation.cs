using OperationSystem.Handlers.Units;
using OperationSystem.Units;

namespace OperationSystem.Operations.Units
{
    public interface IUnitOperation : IOperation
    {
        void RunOperation(IUnitOperationHandler handler);
    }
}