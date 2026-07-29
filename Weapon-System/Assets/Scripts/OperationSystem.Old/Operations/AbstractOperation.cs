using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Coroutine;
using OperationSystem.Handlers;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;

namespace OperationSystem.Operations
{
    public abstract class AbstractOperation : IOperation
    {
        protected const int AcquireLocksTimeout = 10;
        
        private readonly IEnumerable<IOperationData> _data;
        
        protected readonly IEnumerable<IOperationMiddleware> Middlewares;
        protected YieldCoroutine? Coroutine;

        public OperationIdentifier Identifier { get; }
        public bool IsCompleted { get; protected set; }
        public bool IsCompletedSuccessfully => Exception == null;
        public Exception? Exception { get; private set; }

        protected AbstractOperation(
            OperationIdentifier identifier, 
            IEnumerable<IOperationMiddleware> middlewares,
            params IOperationData[] data)
        {
            Identifier = identifier;
            _data = data;
            Middlewares = middlewares;
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

        public T? TryGetData<T>()
            where T : IOperationData
        {
            return _data.OfType<T>().FirstOrDefault();
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

        protected IEnumerable<IOperationMiddleware> EnumerateValidMiddlewares()
        {
            return Middlewares.Where(middleware => middleware.IsValidTaget(this));
        }
    }
}