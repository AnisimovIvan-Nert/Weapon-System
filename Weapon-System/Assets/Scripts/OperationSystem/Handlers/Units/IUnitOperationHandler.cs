using System.Collections;
using OperationSystem.Units;

namespace OperationSystem.Handlers.Units
{
    public interface IUnitOperationHandler<T> : IOperationHandler
        where T : IUnit
    {
        T? Unit { get; }

        IEnumerator SetUnit(T? unit);
    }
}