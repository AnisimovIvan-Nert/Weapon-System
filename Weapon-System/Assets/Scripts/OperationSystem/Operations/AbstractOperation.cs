using System;
using System.Collections;
using Coroutine;
using OperationSystem.Handlers;

namespace OperationSystem.Operations
{
    public abstract class AbstractOperation : IOperation
    {
        protected const int AcquireLocksTimeout = 10;
        
        protected YieldCoroutine? Coroutine;

        public Guid Identifier { get; }
        public bool IsCompleted { get; protected set; }
        public bool IsCompletedSuccessfully => Exception == null;
        public Exception? Exception { get; private set; }

        protected AbstractOperation(Guid identifier)
        {
            Identifier = identifier;
        }

        public virtual void Increment(IOperationContext context)
        {
            Coroutine ??= IncrementEnumerator(context).ToCoroutine();

            while (Coroutine.MoveNext())
            {
                if (Coroutine.InContinueState())
                    continue;

                return;
            }

            IsCompleted = true;
            AppendException(Coroutine.Exception);
        }

        public virtual void RunOperation(IOperationHandler handler)
        {
            handler.OperationRunner.RunOperation(this);
        }

        protected abstract IEnumerator IncrementEnumerator(IOperationContext context);

        protected IEnumerator WaitCoroutine(YieldCoroutine coroutine)
        {
            yield return coroutine;

            var isCompletedSuccessfully = coroutine.IsCompletedSuccessfully();

            if (!isCompletedSuccessfully)
                AppendException(coroutine.Exception);
        }

        protected void AppendException(Exception? exception)
        {
            if (exception == null)
                return;

            if (Exception == null)
            {
                Exception = exception;
                return;
            }

            Exception = new AggregateException(Exception, exception);
        }
    }
}