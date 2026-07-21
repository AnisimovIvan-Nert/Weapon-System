using System;
using System.Collections;
using Coroutine;

namespace Weapons.Operations
{
    public abstract class AbstractOperation : IOperation
    {
        private YieldCoroutine? _coroutine;

        public Guid Identifier { get; }
        public bool IsCompleted { get; private set; }
        public Exception? Exception { get; private set; }

        protected AbstractOperation(Guid identifier)
        {
            Identifier = identifier;
        }

        public virtual void Increment()
        {
            _coroutine ??= IncrementEnumerator().ToCoroutine();

            while (_coroutine.MoveNext())
            {
                if (_coroutine.Current is IEnumerator)
                    continue;

                return;
            }

            IsCompleted = true;
            AppendException(_coroutine.Exception);
        }

        protected abstract IEnumerator IncrementEnumerator();

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