using System.Collections.Generic;
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
        private readonly IUnitOperationHandler _sender;
        private readonly IUnitOperationHandler _receiver;

        public MovingObjectOperation(
            OperationIdentifier identifier,
            IOperationExecutor executor,
            IOperationTarget target,
            IUnitOperationHandler sender,
            IUnitOperationHandler receiver,
            params IOperationMiddleware[] middlewares)
            : base(identifier, middlewares, executor, target)
        {
            _sender = sender;
            _receiver = receiver;
        }

        protected override ICollection<IStagedOperation> CreateAndRunOrchestratedOperations()
        {
            var executor = this.GetData<IOperationExecutor>();
            var target = this.GetData<IOperationTarget>();

            var sendOperation = new SendObjectUnitOperation(Identifier, executor, target, Middlewares);
            var receiveObjectOperation = new ReceiveObjectOperation(Identifier, executor, target, Middlewares);

            sendOperation.RunOperation(_sender);
            receiveObjectOperation.RunOperation(_receiver);

            return new List<IStagedOperation> { sendOperation, receiveObjectOperation };
        }
    }
}