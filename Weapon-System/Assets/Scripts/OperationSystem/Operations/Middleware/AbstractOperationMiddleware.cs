using System.Collections;
using OperationSystem.Handlers;
using OperationSystem.Operations.Tags;

namespace OperationSystem.Operations.Middleware
{
    public abstract class AbstractOperationMiddleware<T> : IOperationMiddleware
        where T : IOperationTag
    {
        public virtual bool IsValidTaget(IOperation operation)
        {
            return operation is T;
        }

        public virtual IEnumerator Initialization(IOperation operation, IOperationContext context, IOperationHandler handler)
        {
            yield break;
        }

        public virtual IEnumerator Validate(IOperation operation, IOperationContext context, IOperationHandler handler)
        {
            yield break;
        }

        public virtual IEnumerator TryAcquireLocks(IOperation operation, IOperationContext context, IOperationHandler handler)
        {
            yield break;
        }

        public virtual IEnumerator ReleaseLocks(IOperation operation, IOperationContext context, IOperationHandler handler)
        {
            yield break;
        }

        public virtual IEnumerator RecordMutations(IOperation operation, IOperationContext context, IOperationHandler handler)
        {
            yield break;
        }

        public virtual IEnumerator Execute(IOperation operation, IOperationContext context, IOperationHandler handler)
        {
            yield break;
        }

        public virtual IEnumerator Complete(IOperation operation, IOperationContext context, IOperationHandler handler)
        {
            yield break;
        }

        public virtual IEnumerator Cancel(IOperation operation, IOperationContext context, IOperationHandler handler)
        {
            yield break;
        }
        
        public virtual void Dispose(IOperation operation, IOperationContext context, IOperationHandler handler)
        {
        }
    }
}