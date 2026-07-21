using Weapons.Operations;
using Weapons.Units;

namespace Weapons.UnitHandlers
{
    public interface IUnitHandler<T>
        where T : IUnit
    {
        T Unit { get; }
        IOperationRunner OperationRunner { get; }

        void Update();
    }
}