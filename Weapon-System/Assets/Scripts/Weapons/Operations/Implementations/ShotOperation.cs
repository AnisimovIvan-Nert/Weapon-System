using System;
using System.Collections;
using Coroutine;
using Weapons.Units.Weapons;

namespace Weapons.Operations.Implementations
{
    public class ShotOperation : AbstractOperation<IWeapon>
    {
        private YieldCoroutine? _controllerCoroutine;
        private YieldCoroutine? _animatorCoroutine;
        
        private YieldCoroutine? _controllerCancelCoroutine;
        private YieldCoroutine? _animatorCancelCoroutine;

        public ShotOperation(Guid identifier, IWeapon unit) 
            : base(identifier, unit)
        {
        }

        protected override IEnumerator Start()
        {
            _controllerCoroutine = Unit.Controller.PerformShot(Unit, this).ToCoroutine();
            var baseStart = base.Start();
            yield return baseStart;
        }

        protected override IEnumerator Progress()
        {
            if (_controllerCoroutine == null)
                throw new InvalidOperationException();
            
            var failure = OperationStatus.InCancellation;
            
            yield return WaitCoroutine(_controllerCoroutine, null, failure);
            
            _animatorCoroutine ??= Unit.Animator.PerformShot(Unit, this).ToCoroutine();
            yield return WaitCoroutine(_animatorCoroutine, null, failure);
            
            yield return base.Progress();
        }

        protected override IEnumerator Canceling()
        {
            if (_animatorCoroutine != null)
            {
                _animatorCancelCoroutine ??= Unit.Animator.CancelShot(Unit, this).ToCoroutine();
                yield return WaitCoroutine(_animatorCancelCoroutine);
            }

            if (_controllerCoroutine != null)
            {
                _controllerCancelCoroutine ??= Unit.Controller.CancelShot(Unit, this).ToCoroutine();
                yield return WaitCoroutine(_controllerCancelCoroutine);
            }

            yield return base.Canceling();
        }
    }
}