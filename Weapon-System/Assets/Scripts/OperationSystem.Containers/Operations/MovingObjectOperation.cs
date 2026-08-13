using System.Collections.Generic;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Units;

namespace OperationSystem.Containers.Operations
{
    public class MovingObjectOperation : AbstractOrchestratorOperation
    {
        private readonly Unit _sender;
        private readonly Unit _receiver;

        public MovingObjectOperation(
            OperationIdentifier identifier,
            IOperationExecutor executor,
            IOperationTarget target,
            Unit sender,
            Unit receiver)
            : base(identifier, executor, target)
        {
            _sender = sender;
            _receiver = receiver;
        }

        protected override ICollection<IOperation> CreateAndRunOrchestratedOperations()
        {
            var executor = this.GetData<IOperationExecutor>();
            var target = this.GetData<IOperationTarget>();

            var senderUnit = new OperationUnit(_sender);
            var sendOperation = new SendObjectUnitOperation(Identifier, senderUnit, executor, target);

            var receiverUnit = new OperationUnit(_receiver);
            var receiveObjectOperation = new ReceiveObjectOperation(Identifier, receiverUnit, executor, target);

            RunOperation(sendOperation, OperationStaging.Manual);
            RunOperation(receiveObjectOperation, OperationStaging.Manual);

            return new List<IOperation> { sendOperation, receiveObjectOperation };
        }
    }
}