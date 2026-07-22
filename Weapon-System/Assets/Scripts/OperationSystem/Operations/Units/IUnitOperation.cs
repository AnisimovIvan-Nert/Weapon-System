using OperationSystem.Handlers.Units;
using OperationSystem.Units;

namespace OperationSystem.Operations.Units
{
    public interface IUnitOperation<T> : IOperation
        where T : IUnit
    {
        void RunOperation(IUnitOperationHandler<T> handler);
    }
}