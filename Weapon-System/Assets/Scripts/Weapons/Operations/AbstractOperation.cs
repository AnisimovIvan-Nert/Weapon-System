using System;
using System.Collections;
using Coroutine;
using Weapons.UnitHandlers;
using Weapons.Units;

namespace Weapons.Operations
{
    public abstract class AbstractOperation<T> : IOperation<T>
        where T : IUnit
    {
        protected const int AcquireLocksTimeout = 100;
        
        private YieldCoroutine? _coroutine;

        protected IUnitHandler<T>? NullableHandler;
        protected IUnitHandler<T> Handler => NullableHandler ?? throw new InvalidOperationException();

        public Guid Identifier { get; }
        public bool IsCompleted { get; private set; }
        public bool IsCompletedSuccessfully => Exception == null;
        public Exception? Exception { get; private set; }

        protected AbstractOperation(Guid identifier)
        {
            Identifier = identifier;
        }
        
        public void RunOperation(IUnitHandler<T> handler)
        {
            if (NullableHandler != null)
                throw new InvalidOperationException();
            NullableHandler = handler;
            handler.OperationRunner.RunOperation(this);
        }

        public virtual void Increment(IOperationContext context)
        {
            _coroutine ??= IncrementEnumerator(context).ToCoroutine();

            while (_coroutine.MoveNext())
            {
                if (_coroutine.Current is IEnumerator)
                    continue;

                return;
            }

            IsCompleted = true;
            AppendException(_coroutine.Exception);
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