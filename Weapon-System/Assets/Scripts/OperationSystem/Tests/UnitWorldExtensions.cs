using OperationSystem.Assets;
using OperationSystem.Handlers;
using OperationSystem.Units;

namespace OperationSystem.Tests
{
    public static class UnitWorldExtensions
    {
        public static Unit CreateUnitWithDefaultHandler(this UnitWorld world, IAsset asset, int? timeout = null)
        {
            var handler = new OperationHandler(world);
            return handler.SetAndReturnUnit(asset, timeout);
        }
    }
}