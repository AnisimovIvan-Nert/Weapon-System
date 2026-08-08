using System.Collections;
using OperationSystem.Handlers;

namespace OperationSystem.Operations.Middleware
{
    public interface IOperationMiddleware
    {
        bool IsValidTaget(IOperation operation);
        
        IEnumerator Validate(IOperation operation, IOperationContext context, IOperationHandler handler);
        IEnumerator TryAcquireLocks(IOperation operation, IOperationContext context, IOperationHandler handler);
        IEnumerator RecordPossibleMutations(IOperation operation, IOperationContext context, IOperationHandler handler);
        IEnumerator Execute(IOperation operation, IOperationContext context, IOperationHandler handler);
    }
}