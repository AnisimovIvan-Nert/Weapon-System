using System;
using System.Collections;
using OperationSystem.Handlers;
using OperationSystem.Handlers.Units;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Staged;

namespace OperationSystem.Operations.Units
{
    public abstract class AbstractStagedUnitOperation
        : AbstractStagedOperation
        , IUnitOperation
    {
        protected IUnitOperationHandler? NullableHandler;
        protected IUnitOperationHandler Handler => NullableHandler ?? throw new InvalidOperationException();
        
        protected AbstractStagedUnitOperation(
            OperationIdentifier identifier, 
            IOperationMiddleware[] middlewares, 
            params IOperationData[] data) 
            : base(identifier, middlewares, data)
        {
        }
        
        public void RunOperation(IUnitOperationHandler handler)
        {
            if (NullableHandler != null)
                throw new InvalidOperationException();
            NullableHandler = handler;
            var context = handler.CreateContext();
            handler.OperationRunner.RunOperation(this, context);
        }

        public override void RunOperation(IOperationHandler handler)
        {
            if (handler is not IUnitOperationHandler typedHandler)
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