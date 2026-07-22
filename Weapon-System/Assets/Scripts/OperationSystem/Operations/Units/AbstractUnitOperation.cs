using System;
using OperationSystem.Handlers;
using OperationSystem.Handlers.Units;
using OperationSystem.Units;

namespace OperationSystem.Operations.Units
{
    public abstract class AbstractUnitOperation<T> 
        : AbstractOperation
        , IUnitOperation<T>
        where T : IUnit
    {
        protected IUnitOperationHandler<T>? NullableHandler;
        protected IUnitOperationHandler<T> Handler => NullableHandler ?? throw new InvalidOperationException();

        protected AbstractUnitOperation(Guid identifier) : base(identifier)
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
    }
}