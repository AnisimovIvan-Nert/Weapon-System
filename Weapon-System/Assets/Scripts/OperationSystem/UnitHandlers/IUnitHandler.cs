using System.Collections;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.UnitHandlers
{
    public interface IUnitHandler<T>
        where T : IUnit
    {
        T? Unit { get; }
        IOperationRunner OperationRunner { get; }

        void Update();

        IEnumerator SetUnit(T? unit);
    }
}