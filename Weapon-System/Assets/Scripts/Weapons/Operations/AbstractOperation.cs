using System;
using System.Collections;
using Coroutine;

namespace Weapons.Operations
{
    public abstract class AbstractOperation<T> : IOperation<T>
        where T : IUnit
    {
        private YieldCoroutine? _coroutine;

        public OperationStatus Status { get; private set; }
        public Exception? Exception { get; private set; }

        public virtual bool Increment(T unit)
        {
            if (Status == OperationStatus.None)
                Status |= OperationStatus.Pending;

            _coroutine ??= IncrementEnumerator(unit).ToCoroutine();

            while (_coroutine.MoveNext())
            {
                if (_coroutine.Current is IEnumerator)
                    continue;

                if (_coroutine.Current is OperationStatus status)
                {
                    Status = status;
                    _coroutine = IncrementEnumerator(unit).ToCoroutine();
                    continue;
                }

                return true;
            }

            AppendException(_coroutine.Exception);
            return false;
        }

        private IEnumerator IncrementEnumerator(T unit)
        {
            if (Status & OperationStatus.Pending)
                yield return Start(unit);

            if (Status & OperationStatus.InProgress)
                yield return Progress(unit);

            if (Status & OperationStatus.InCancellation)
                yield return Canceling(unit);

            if (Status & OperationStatus.Complete)
                yield return PrepareForDestroying(unit);

            if (Status & OperationStatus.ReadyForDestroying)
                yield return Destroy(unit);
        }

        protected IEnumerator WaitCoroutine(
            YieldCoroutine coroutine,
            object? success,
            object? failure,
            bool yieldNull = true)
        {
            yield return coroutine;

            var isCompletedSuccessfully = coroutine.IsCompletedSuccessfully();

            if (!isCompletedSuccessfully)
                AppendException(coroutine.Exception);
            
            var result = isCompletedSuccessfully
                ? success
                : failure;

            if (result != null || yieldNull)
                yield return result;
        }

        protected void AppendException(Exception? exception)
        {
            if (exception == null)
                return;

            Status |= OperationStatus.WithException;

            if (Exception == null)
            {
                Exception = exception;
                return;
            }

            Exception = new AggregateException(Exception, exception);
        }

        protected virtual IEnumerator Start(T unit)
        {
            yield return OperationStatus.InProgress;
        }

        protected virtual IEnumerator Progress(T unit)
        {
            yield return OperationStatus.Complete;
        }

        protected virtual IEnumerator Canceling(T unit)
        {
            yield return OperationStatus.Complete;
        }

        protected virtual IEnumerator PrepareForDestroying(T unit)
        {
            yield return OperationStatus.ReadyForDestroying;
        }

        protected virtual IEnumerator Destroy(T unit)
        {
            yield return OperationStatus.Destroying;
        }
    }
}