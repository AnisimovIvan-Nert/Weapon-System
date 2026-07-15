using System.Threading.Tasks;

namespace Weapons.Operations
{
    public class ShotOperation : IWeaponOperation
    {
        public OperationState State { get; private set; }
        public OperationResult Result { get; private set; }

        private Task? _controllerTask;
        private Task? _animatorTask;
        
        private Task? _controllerCancelTask;
        private Task? _animatorCancelTask;
        
        public void Increment(IWeapon weapon)
        {
            if (State == OperationState.Pending)
                Start(weapon);

            if (State == OperationState.InProgress)
                Progress(weapon);
            
            if (State == OperationState.InCancellation)
                Canceling(weapon);

            if (State == OperationState.Complete)
                State = OperationState.ReadyForDestroying;

            if (State == OperationState.ReadyForDestroying)
                State = OperationState.Destroying;
        }

        private void Start(IWeapon weapon)
        {
            State = OperationState.InProgress;
            _controllerTask = weapon.Controller.PerformShot(weapon, this);
        }

        private void Progress(IWeapon weapon)
        {
            if (_controllerTask is not { IsCompleted: true })
                return;

            if (!_controllerTask.IsCompletedSuccessfully)
            {
                State = OperationState.InCancellation;
                Result = OperationResult.Failure;
                return;
            }

            _animatorTask ??= weapon.Animator.PerformShot(weapon, this);
            
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

        private void Canceling(IWeapon weapon)
        {
            if (_animatorTask != null)
                _animatorCancelTask ??= weapon.Animator.CancelShot(weapon, this);
            
            if (_animatorCancelTask is { IsCompleted: false })
                return;

            if (_controllerTask != null)
                _controllerCancelTask ??= weapon.Controller.CancelShot(weapon, this);
            
            if (_controllerTask is { IsCompleted: false })
                return;
            
            State = OperationState.Complete;
        }
    }
}