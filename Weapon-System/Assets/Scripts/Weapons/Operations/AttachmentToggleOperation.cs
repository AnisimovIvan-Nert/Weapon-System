using System.Threading.Tasks;

namespace Weapons.Operations
{
    public class AttachmentToggleOperation : IOperation<IAttachment>
    {
        public OperationState State { get; private set; }
        public OperationResult Result { get; private set; }

        private Task? _controllerTask;
        private Task? _animatorTask;
        
        private Task? _controllerCancelTask;
        private Task? _animatorCancelTask;
        
        public void Increment(IAttachment unit)
        {
            if (State == OperationState.Pending)
                Start(unit);

            if (State == OperationState.InProgress)
                Progress(unit);
            
            if (State == OperationState.InCancellation)
                Canceling(unit);

            if (State == OperationState.Complete)
                State = OperationState.ReadyForDestroying;

            if (State == OperationState.ReadyForDestroying)
                State = OperationState.Destroying;
        }

        private void Start(IAttachment unit)
        {
            State = OperationState.InProgress;
            _controllerTask = unit.Controller.PerformToggle(unit, this);
        }

        private void Progress(IAttachment unit)
        {
            if (_controllerTask is not { IsCompleted: true })
                return;

            if (!_controllerTask.IsCompletedSuccessfully)
            {
                State = OperationState.InCancellation;
                Result = OperationResult.Failure;
                return;
            }

            _animatorTask ??= unit.Animator.PerformToggle(unit, this);
            
            if (_animatorTask is not { IsCompleted: true })
                return;
            
            if (!_animatorTask.IsCompletedSuccessfully)
            {
                State = OperationState.InCancellation;
                Result = OperationResult.Failure;
                return;
            }

            State = OperationState.Complete;
            Result = OperationResult.Success;
        }

        private void Canceling(IAttachment unit)
        {
            if (_animatorTask != null)
                _animatorCancelTask ??= unit.Animator.CancelToggle(unit, this);
            
            if (_animatorCancelTask is { IsCompleted: false })
                return;

            if (_controllerTask != null)
                _controllerCancelTask ??= unit.Controller.CancelToggle(unit, this);
            
            if (_controllerTask is { IsCompleted: false })
                return;
            
            State = OperationState.Complete;
        }
    }
}