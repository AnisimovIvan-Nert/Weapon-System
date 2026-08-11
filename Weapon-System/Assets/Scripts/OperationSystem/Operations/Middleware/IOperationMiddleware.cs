using System.Collections;
using OperationSystem.Handlers;

namespace OperationSystem.Operations.Middleware
{
    public interface IOperationMiddleware
    {
        bool IsValidTaget(IOperation operation);
        
        IEnumerator Initialization(IOperation operation, IOperationContext context, IOperationHandler handler);
        IEnumerator Validate(IOperation operation, IOperationContext context, IOperationHandler handler);
        IEnumerator TryAcquireLocks(IOperation operation, IOperationContext context, IOperationHandler handler);
        IEnumerator ReleaseLocks(IOperation operation, IOperationContext context, IOperationHandler handler);
        IEnumerator RecordMutations(IOperation operation, IOperationContext context, IOperationHandler handler);
        IEnumerator Execute(IOperation operation, IOperationContext context, IOperationHandler handler);
        IEnumerator Complete(IOperation operation, IOperationContext context, IOperationHandler handler);
        IEnumerator Cancel(IOperation operation, IOperationContext context, IOperationHandler handler);
        void Dispose(IOperation operation, IOperationContext context, IOperationHandler handler);
    }
}