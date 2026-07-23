using System;
using System.Collections;
using System.Collections.Generic;
using OperationSystem.Handlers;
using OperationSystem.Handlers.Units;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Staged;
using OperationSystem.Units;

namespace OperationSystem.Operations.Units
{
    public abstract class AbstractStagedUnitOperation<T>
        : AbstractStagedOperation
        , IUnitOperation<T>
        where T : IUnit
    {
        protected IUnitOperationHandler<T>? NullableHandler;
        protected IUnitOperationHandler<T> Handler => NullableHandler ?? throw new InvalidOperationException();
        
        protected AbstractStagedUnitOperation(
            Guid identifier, 
            IEnumerable<IOperationMiddleware> middlewares, 
            params IOperationData[] data) 
            : base(identifier, middlewares, data)
        {
        }
        
        public void RunOperation(IUnitOperationHandler<T> handler)
        {
            if (NullableHandler != null)
                throw new InvalidOperationException();
            NullableHandler = handler;
            handler.OperationRunner.RunOperation(this);
        }

        public override void RunOperation(IOperationHandler handler)
        {
            if (handler is not IUnitOperationHandler<T> typedHandler)
                throw new InvalidOperationException();
            
            RunOperation(typedHandler);
        }

        protected override IEnumerator ValidateEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.Validate(this, Context, Handler);
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.TryAcquireLocks(this, Context, Handler);
        }

        protected override IEnumerator RecordPossibleMutationsEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.RecordPossibleMutations(this, Context, Handler);
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            foreach (var middleware in EnumerateValidMiddlewares())
                yield return middleware.Execute(this, Context, Handler);
        }
    }
}