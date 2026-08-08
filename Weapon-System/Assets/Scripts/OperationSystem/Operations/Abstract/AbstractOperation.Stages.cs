using System.Collections;

namespace OperationSystem.Operations.Abstract
{
    public abstract partial class AbstractOperation
    {
        protected virtual IEnumerator InitializationEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.Initialization(this, Context, Handler);
        }
        
        protected virtual IEnumerator ValidateEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.Validate(this, Context, Handler);
        }
        
        protected virtual IEnumerator TryAcquireLocksEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.TryAcquireLocks(this, Context, Handler);
        }

        protected virtual IEnumerator ReleaseLocksEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.ReleaseLocks(this, Context, Handler);
            
            Context.ReleaseAll();
        }
        
        protected virtual IEnumerator RecordMutationsEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.RecordMutations(this, Context, Handler);
        }
        
        protected virtual IEnumerator ExecuteEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.Execute(this, Context, Handler);
        }

        protected virtual IEnumerator CompleteEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.Complete(this, Context, Handler);
        }

        protected virtual IEnumerator CancelEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.Cancel(this, Context, Handler);
        }
    }
}