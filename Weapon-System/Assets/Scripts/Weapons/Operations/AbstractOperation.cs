using System;
using System.Collections;
using Coroutine;

namespace Weapons.Operations
{
    public abstract class AbstractOperation<TTarget, TSource> : AbstractOperation
        where TTarget : IUnit
        where TSource : IUnit
    {
        protected readonly TTarget Target;
        protected readonly TSource Source;
        
        protected AbstractOperation(Guid identifier, TTarget target, TSource source)
            : base(identifier)
        {
            Target = target;
            Source = source;
        }
    }
    
    public abstract class AbstractOperation<T> : AbstractOperation
        where T : IUnit
    {
        protected readonly T Unit;
        
        protected AbstractOperation(Guid identifier, T unit)
            : base(identifier)
        {
            Unit = unit;
        }
    }
    
     public abstract class AbstractOperation : IOperation
    {
        private YieldCoroutine? _coroutine;

        public Guid Identifier { get; }
        public OperationStatus Status { get; private set; }
        public Exception? Exception { get; private set; }
        
        protected AbstractOperation(Guid identifier)
        {
            Identifier = identifier;
        }

        public virtual bool Increment()
        {
            if (Status == OperationStatus.None)
                Status |= OperationStatus.Pending;

            _coroutine ??= IncrementEnumerator().ToCoroutine();

            while (_coroutine.MoveNext())
            {
                if (_coroutine.Current is IEnumerator)
                    continue;

                if (_coroutine.Current is OperationStatus status)
                {
                    Status = status;
                    _coroutine = IncrementEnumerator().ToCoroutine();
                    continue;
                }

                return true;
            }

            AppendException(_coroutine.Exception);
            return false;
        }

        private IEnumerator IncrementEnumerator()
        {
            if (Status & OperationStatus.Pending)
                yield return Start();

            if (Status & OperationStatus.InProgress)
                yield return Progress();

            if (Status & OperationStatus.InCancellation)
                yield return Canceling();

            if (Status & OperationStatus.Complete)
                yield return PrepareForDestroying();

            if (Status & OperationStatus.ReadyForDestroying)
                yield return Destroy();
        }

        protected IEnumerator WaitCoroutine(
            YieldCoroutine coroutine,
            object? success = null,
            object? failure = null,
            bool yieldNull = false)
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

        protected virtual IEnumerator Start()
        {
            yield return OperationStatus.InProgress;
        }

        protected virtual IEnumerator Progress()
        {
            yield return OperationStatus.Complete;
        }

        protected virtual IEnumerator Canceling()
        {
            yield return OperationStatus.Complete;
        }

        protected virtual IEnumerator PrepareForDestroying()
        {
            yield return OperationStatus.ReadyForDestroying;
        }

        protected virtual IEnumerator Destroy()
        {
            yield return OperationStatus.Destroying;
        }
    }
}