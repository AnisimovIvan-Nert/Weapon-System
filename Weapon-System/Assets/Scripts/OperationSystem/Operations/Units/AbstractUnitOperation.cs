using System;
using System.Collections.Generic;
using OperationSystem.Handlers;
using OperationSystem.Handlers.Units;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;

namespace OperationSystem.Operations.Units
{
    public abstract class AbstractUnitOperation<T> 
        : AbstractOperation
        , IUnitOperation
    {
        protected IUnitOperationHandler? NullableHandler;
        protected IUnitOperationHandler Handler => NullableHandler ?? throw new InvalidOperationException();
        
        protected AbstractUnitOperation(
            OperationIdentifier identifier, 
            IEnumerable<IOperationMiddleware> middlewares, 
            params IOperationData[] data) 
            : base(identifier, middlewares, data)
        {
        }
        
        public void RunOperation(IUnitOperationHandler handler)
        {
            if (NullableHandler != null)
                throw new InvalidOperationException();
            NullableHandler = handler;
            handler.OperationRunner.RunOperation(this);
        }

        public override void RunOperation(IOperationHandler handler)
        {
            if (handler is not IUnitOperationHandler typedHandler)
                throw new InvalidOperationException();
            
            RunOperation(typedHandler);
        }
    }
}