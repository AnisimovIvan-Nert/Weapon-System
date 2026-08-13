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
        
        protected IOperationContext Context = null!;
        protected IOperationRunner Runner = null!;
        protected IOperationMiddleware[] UserMiddlewares = null!;
        
        private IOperationMiddleware[] _validUserMiddlewares = null!;
        private IOperationMiddleware[] _worldMiddlewares = null!;

        protected IEnumerable<IOperationMiddleware> Middlewares => _validUserMiddlewares.Concat(_worldMiddlewares);

        public OperationIdentifier Identifier { get; }
        public bool IsCompleted { get; protected set; }
        public bool IsCompletedSuccessfully => Exception is null or OperationForcedComplete;
        public Exception? Exception { get; private set; }
        public IOperationResult? OperationResult { get; private set; }

        protected AbstractOperation(OperationIdentifier identifier, params IOperationData[] data)
        {
            Identifier = identifier;
            _data = data;
        }

        public virtual IOperationContext CreateContext(UnitWorld world) => new OperationContext(world);

        public virtual void RunOperation(
            IOperationRunner operationRunner,
            UnitWorld world,
            OperationStaging staging = OperationStaging.Auto,
            params IOperationMiddleware[] middlewares)
        {
            _staging = staging;
            Runner = operationRunner;
            UserMiddlewares = middlewares;
            _validUserMiddlewares = middlewares.Where(middleware => middleware.IsValidTaget(this)).ToArray();
            _worldMiddlewares = world.Middlewares.Where(middleware => middleware.IsValidTaget(this)).ToArray();
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
        
        public void SetResult(IOperationResult? result)
        {
            OperationResult = result;
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

        protected void SetCompleted()
        {
            Dispose();
            IsCompleted = true;
        }

        protected void RunOperation(IOperation operation, OperationStaging staging = OperationStaging.Auto)
        {
            operation.RunOperation(Runner, Context.World, staging, UserMiddlewares);
        }
    }
}