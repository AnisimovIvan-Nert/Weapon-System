using System.Collections;

namespace OperationSystem.Operations.Middleware
{
    public interface IOperationMiddleware
    {
        bool IsValidTaget(IOperation operation);
        
        IEnumerator Initialization(IOperation operation, IOperationContext context);
        IEnumerator Validate(IOperation operation, IOperationContext context);
        IEnumerator TryAcquireLocks(IOperation operation, IOperationContext context);
        IEnumerator ReleaseLocks(IOperation operation, IOperationContext context);
        IEnumerator RecordMutations(IOperation operation, IOperationContext context);
        IEnumerator Execute(IOperation operation, IOperationContext context);
        IEnumerator Complete(IOperation operation, IOperationContext context);
        IEnumerator Cancel(IOperation operation, IOperationContext context);
        void Dispose(IOperation operation, IOperationContext context);
    }
}