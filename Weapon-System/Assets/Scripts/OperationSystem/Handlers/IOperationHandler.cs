using OperationSystem.Component;
using OperationSystem.Component.Types;
using OperationSystem.Operations;

namespace OperationSystem.Handlers
{
    public interface IOperationHandler
    {
        IOperationRunner OperationRunner { get; }
        void Update();
    }
}