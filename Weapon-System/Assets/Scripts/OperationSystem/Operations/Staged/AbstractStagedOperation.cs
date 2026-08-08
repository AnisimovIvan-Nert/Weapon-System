using System;
using System.Collections;
using System.Threading.Tasks;
using Coroutine;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;

namespace OperationSystem.Operations.Staged
{
    public abstract class AbstractStagedOperation
        : AbstractOperation
        , IStagedOperation
    {
        private Task<bool>? _coroutineTask;
        private TaskCompletionSource<bool>? _taskCompletionSource;

        private IOperationContext? _context;

        protected IOperationContext Context => _context ?? throw new InvalidOperationException();
        
        protected AbstractStagedOperation(
            OperationIdentifier identifier, 
            IOperationMiddleware[] middlewares, 
            params IOperationData[] data) 
            : base(identifier, middlewares, data)
        {
        }

        public override void Increment(IOperationContext context)
        {
            if (Coroutine == null || _taskCompletionSource == null)
                return;

            _context = context;
            
            while (Coroutine.MoveNext())
            {
                if (Coroutine.InContinueState())
                    continue;

                return;
            }

            if (Coroutine.Exception == null)
                _taskCompletionSource.TrySetResult(true);
            else
                _taskCompletionSource.TrySetException(Coroutine.Exception);

            Coroutine = null;
        }

        public Task Validate() => CreateCoroutineTask(ValidateEnumerator());
        public Task TryAcquireLocks() => CreateCoroutineTask(TryAcquireLocksEnumerator());
        public Task ReleaseLocks() => CreateCoroutineTask(ReleaseLocksEnumerator());
        public Task RecordPossibleMutations() => CreateCoroutineTask(RecordPossibleMutationsEnumerator());
        public Task Execute() => CreateCoroutineTask(ExecuteEnumerator());
        public Task Complete() => CreateCoroutineTask(CompleteEnumerator());
        public Task Cancel(Exception exception) => CreateCoroutineTask(CancelEnumerator(exception));

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
            Context.ReleaseAll();
            yield break;
        }
        
        protected virtual IEnumerator RecordPossibleMutationsEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.RecordPossibleMutations(this, Context, Handler);
        }
        
        protected virtual IEnumerator ExecuteEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.Execute(this, Context, Handler);
        }

        protected virtual IEnumerator CompleteEnumerator()
        {
            IsCompleted = true;
            yield break;
        }

        protected virtual IEnumerator CancelEnumerator(Exception exception)
        {
            AppendException(exception);
            IsCompleted = true;
            yield break;
        }
        
        protected override IEnumerator IncrementEnumerator(IOperationContext context)
        {
            throw new InvalidOperationException();
        }

        private async Task CreateCoroutineTask(IEnumerator enumerator)
        {
            if (_coroutineTask is { IsCompleted: false })
                await _coroutineTask;

            _taskCompletionSource = new TaskCompletionSource<bool>();
            _coroutineTask = _taskCompletionSource.Task;
            
            Coroutine = enumerator.ToCoroutine();

            await _coroutineTask;
        }
    }
}