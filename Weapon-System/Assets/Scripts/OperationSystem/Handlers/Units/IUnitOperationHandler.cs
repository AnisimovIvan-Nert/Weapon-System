using System.Collections;
using OperationSystem.Units;

namespace OperationSystem.Handlers.Units
{
    public interface IUnitOperationHandler : IOperationHandler
    {
        IUnit? Unit { get; }
    }
    
    public interface IUnitOperationHandler<T> : IUnitOperationHandler
        where T : IUnit
    {
        new T? Unit { get; }

        IEnumerator SetUnit(T? unit);
    }
}