using System;
using System.Collections;
using Coroutine;
using Weapons.Units.Weapons;

namespace Weapons.Operations.Shot
{
    public class ShotOperation : AbstractOperation<IWeapon>
    {
        private YieldCoroutine? _controllerCoroutine;
        private YieldCoroutine? _animatorCoroutine;
        
        private YieldCoroutine? _controllerCancelCoroutine;
        private YieldCoroutine? _animatorCancelCoroutine;

        protected override IEnumerator Start(IWeapon unit)
        {
            _controllerCoroutine = unit.Controller.PerformShot(unit, this).ToCoroutine();
            var baseStart = base.Start(unit);
            yield return baseStart;
        }

        protected override IEnumerator Progress(IWeapon unit)
        {
            if (_controllerCoroutine == null)
                throw new InvalidOperationException();
            
            var failure = OperationStatus.InCancellation;
            
            yield return WaitCoroutine(_controllerCoroutine, null, failure, false);
            
            _animatorCoroutine ??= unit.Animator.PerformShot(unit, this).ToCoroutine();
            yield return WaitCoroutine(_animatorCoroutine, null, failure, false);
            
            yield return base.Progress(unit);
        }

        protected override IEnumerator Canceling(IWeapon unit)
        {
            if (_animatorCoroutine != null)
            {
                _animatorCancelCoroutine ??= unit.Animator.CancelShot(unit, this).ToCoroutine();
                yield return WaitCoroutine(_animatorCancelCoroutine, null, null, false);
            }

            if (_controllerCoroutine != null)
            {
                _controllerCancelCoroutine ??= unit.Controller.CancelShot(unit, this).ToCoroutine();
                yield return WaitCoroutine(_controllerCancelCoroutine, null, null, false);
            }

            yield return base.Canceling(unit);
        }
    }
}