using System.Collections;

namespace OperationSystem.Operations.Abstract
{
    public abstract partial class AbstractOperation
    {
        protected virtual IEnumerator InitializationEnumerator()
        {
            foreach (var middleware in Middlewares)
                yield return middleware.Initialization(this, Context);
        }
        
        protected virtual IEnumerator ValidateEnumerator()
        {
            foreach (var middleware in Middlewares)
                yield return middleware.Validate(this, Context);
        }
        
        protected virtual IEnumerator TryAcquireLocksEnumerator()
        {
            foreach (var middleware in Middlewares)
                yield return middleware.TryAcquireLocks(this, Context);
        }

        protected virtual IEnumerator ReleaseLocksEnumerator()
        {
            foreach (var middleware in Middlewares)
                yield return middleware.ReleaseLocks(this, Context);
            
            Context.ReleaseAll();
        }
        
        protected virtual IEnumerator RecordMutationsEnumerator()
        {
            foreach (var middleware in Middlewares)
                yield return middleware.RecordMutations(this, Context);
        }
        
        protected virtual IEnumerator ExecuteEnumerator()
        {
            foreach (var middleware in Middlewares)
                yield return middleware.Execute(this, Context);
        }

        protected virtual IEnumerator CompleteEnumerator()
        {
            foreach (var middleware in Middlewares)
                yield return middleware.Complete(this, Context);
        }

        protected virtual IEnumerator CancelEnumerator()
        {
            foreach (var middleware in Middlewares)
                yield return middleware.Cancel(this, Context);
        }
        
        protected virtual void Dispose()
        {
            foreach (var middleware in Middlewares)
                middleware.Dispose(this, Context);
        }
    }
}