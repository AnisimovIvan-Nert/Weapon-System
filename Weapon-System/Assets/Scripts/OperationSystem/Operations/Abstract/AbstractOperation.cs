using System;
using System.Collections.Generic;
using System.Linq;
using Coroutine;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Result;
using OperationSystem.Units;

namespace OperationSystem.Operations.Abstract
{
    public abstract partial class AbstractOperation : IOperation
    {
        private const int AcquireLocksTimeout = 20;
        
        private readonly IEnumerable<IOperationData> _data;
        
        private YieldCoroutine? _coroutine;
        private OperationStaging _staging;
        
        protected readonly IOperationMiddleware[] Middlewares;
        
        protected IOperationContext Context = null!;
        protected IOperationRunner Runner = null!;

        public OperationIdentifier Identifier { get; }
        public bool IsCompleted { get; protected set; }
        public bool IsCompletedSuccessfully => Exception == null;
        public Exception? Exception { get; private set; }
        public IOperationResult? OperationResult { get; protected set; }

        protected AbstractOperation(
            OperationIdentifier identifier,
            IOperationMiddleware[] middlewares,
            params IOperationData[] data)
        {
            Identifier = identifier;
            _data = data;
            Middlewares = middlewares;
        }

        public virtual IOperationContext CreateContext(UnitWorld world) => new OperationContext(world);

        public virtual void RunOperation(
            IOperationRunner operationRunner,
            UnitWorld world,
            OperationStaging staging = OperationStaging.Auto)
        {
            _staging = staging;
            Runner = operationRunner;
            operationRunner.RunOperation(this, world);
        }

        public void Increment(IOperationContext operationContext)
        {
            Context = operationContext;
            
            switch (_staging)
            {
                case OperationStaging.Manual:
                    IncrementManual();
                    break;
                case OperationStaging.Auto:
                    IncrementAuto();
                    break;
                case OperationStaging.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public T? TryGetData<T>()
            where T : IOperationData
        {
            return _data.OfType<T>().FirstOrDefault();
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

        protected void SetCompleted()
        {
            Dispose();
            IsCompleted = true;
        }
    }
}