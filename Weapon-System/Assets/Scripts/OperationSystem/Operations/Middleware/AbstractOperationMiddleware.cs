using System.Collections;
using OperationSystem.Handlers.Units;
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

        public virtual IEnumerator Validate(
            IOperation operation, 
            IOperationContext context, 
            IUnitOperationHandler handler)
        {
            yield break;
        }

        public virtual IEnumerator TryAcquireLocks(
            IOperation operation, 
            IOperationContext context, 
            IUnitOperationHandler handler)
        {
            yield break;
        }

        public virtual IEnumerator RecordPossibleMutations(
            IOperation operation, 
            IOperationContext context, 
            IUnitOperationHandler handler)
        {
            yield break;
        }

        public virtual IEnumerator Execute(
            IOperation operation,
            IOperationContext context,
            IUnitOperationHandler handler)
        {
            yield break;
        }
    }
}