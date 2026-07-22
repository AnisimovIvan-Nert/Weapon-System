using System;
using System.Collections.Generic;
using OperationSystem.Containers.Units;
using OperationSystem.Containers.Units.Containers;
using OperationSystem.Handlers.Units;
using OperationSystem.Operations.Orchestrator;
using OperationSystem.Operations.Staged;
using OperationSystem.Units;

namespace OperationSystem.Containers.Operations
{
    public class MovingObjectOperation : AbstractOrchestratorOperation
    {
        private readonly IPlayer _executor;
        private readonly IUnit _target;

        private readonly IUnitOperationHandler<IContainer> _sender;
        private readonly IUnitOperationHandler<IContainer> _receiver;

        public MovingObjectOperation(
            Guid identifier,
            IPlayer executor,
            IUnit target,
            IUnitOperationHandler<IContainer> sender,
            IUnitOperationHandler<IContainer> receiver)
            : base(identifier)
        {
            _executor = executor;
            _target = target;
            _sender = sender;
            _receiver = receiver;
        }

        protected override ICollection<IStagedOperation> GetOrchestratedOperations()
        {
            var sendOperation = new SendObjectUnitOperation(Identifier, _executor, _target);
            var receiveObjectOperation = new ReceiveObjectOperation(Identifier, _executor, _target);
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