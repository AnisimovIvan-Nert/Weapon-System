using System.Collections;
using OperationSystem.Handlers.Units;

namespace OperationSystem.Operations.Middleware
{
    public interface IOperationMiddleware
    {
        bool IsValidTaget(IOperation operation);
        
        IEnumerator Validate(IOperation operation, IOperationContext context, IUnitOperationHandler handler);
        IEnumerator TryAcquireLocks(IOperation operation, IOperationContext context, IUnitOperationHandler handler);
        IEnumerator RecordPossibleMutations(IOperation operation, IOperationContext context, IUnitOperationHandler handler);
        IEnumerator Execute(IOperation operation, IOperationContext context, IUnitOperationHandler handler);
    }
}