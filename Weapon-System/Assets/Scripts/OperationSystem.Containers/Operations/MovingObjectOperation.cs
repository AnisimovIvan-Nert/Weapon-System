using System;
using System.Collections.Generic;
using System.Linq;
using OperationSystem.Containers.Units.Containers;
using OperationSystem.Handlers.Units;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Orchestrator;
using OperationSystem.Operations.Staged;

namespace OperationSystem.Containers.Operations
{
    public class MovingObjectOperation : AbstractOrchestratorOperation
    {
        private readonly IUnitOperationHandler<IContainer> _sender;
        private readonly IUnitOperationHandler<IContainer> _receiver;

        public MovingObjectOperation(
            Guid identifier,
            IOperationExecutor executor,
            IOperationTarget target,
            IUnitOperationHandler<IContainer> sender,
            IUnitOperationHandler<IContainer> receiver,
            params IOperationMiddleware[] middlewares)
            : base(identifier, middlewares, executor, target)
        {
            _sender = sender;
            _receiver = receiver;
        }

        protected override ICollection<IStagedOperation> GetOrchestratedOperations()
        {
            var executor = this.GetData<IOperationExecutor>();
            var target = this.GetData<IOperationTarget>();
            
            var sendOperation = new SendObjectUnitOperation(Identifier,  executor, target, Middlewares.ToArray());
            var receiveObjectOperation = new ReceiveObjectOperation(Identifier, executor, target, Middlewares.ToArray());
            return new IStagedOperation[] { sendOperation, receiveObjectOperation };
        }

        protected override void RunOrchestratedOperations(ICollection<IStagedOperation> operations)
        {
            var operationArray = (IStagedOperation[])operations;
            var sendOperation = operationArray[0];
            var receiveObjectOperation = operationArray[1];
            
            sendOperation.RunOperation(_sender);
            receiveObjectOperation.RunOperation(_receiver);
        }
    }
}