using System.Collections.Generic;
using OperationSystem.Containers.Tests.Mocks;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;

namespace OperationSystem.Containers.Operations
{
    public class MovingObjectOperation : AbstractOrchestratorOperation
    {
        private readonly ContainerAsset _sender;
        private readonly ContainerAsset _receiver;

        public MovingObjectOperation(
            OperationIdentifier identifier,
            IOperationExecutor executor,
            IOperationTarget target,
            ContainerAsset sender,
            ContainerAsset receiver)
            : base(identifier, executor, target)
        {
            _sender = sender;
            _receiver = receiver;
        }

        protected override ICollection<IOperation> CreateAndRunOrchestratedOperations()
        {
            var executor = this.GetData<IOperationExecutor>();
            var target = this.GetData<IOperationTarget>();

            var senderAsset = new OperationAsset(_sender);
            var sendOperation = new SendObjectUnitOperation(Identifier, senderAsset, executor, target);

            var receiverAsset = new OperationAsset(_receiver);
            var receiveObjectOperation = new ReceiveObjectOperation(Identifier, receiverAsset, executor, target);

            RunOperation(sendOperation, OperationStaging.Manual);
            RunOperation(receiveObjectOperation, OperationStaging.Manual);

            return new List<IOperation> { sendOperation, receiveObjectOperation };
        }
    }
}