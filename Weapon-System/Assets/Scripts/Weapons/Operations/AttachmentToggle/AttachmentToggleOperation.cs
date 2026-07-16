using System;
using System.Collections;
using Coroutine;
using Weapons.Units.Attachments;

namespace Weapons.Operations.AttachmentToggle
{
    public class AttachmentToggleOperation : AbstractOperation<IAttachment>
    {
        private YieldCoroutine? _controllerCoroutine;
        private YieldCoroutine? _animatorCoroutine;
        
        private YieldCoroutine? _controllerCancelCoroutine;
        private YieldCoroutine? _animatorCancelCoroutine;

        protected override IEnumerator Start(IAttachment unit)
        {
            _controllerCoroutine = unit.Controller.PerformToggle(unit, this).ToCoroutine();
            var baseStart = base.Start(unit);
            yield return baseStart;
        }

        protected override IEnumerator Progress(IAttachment unit)
        {
            if (_controllerCoroutine == null)
                throw new InvalidOperationException();
            
            var failure = OperationStatus.InCancellation;
            
            yield return WaitCoroutine(_controllerCoroutine, null, failure, false);
            
            _animatorCoroutine ??= unit.Animator.PerformToggle(unit, this).ToCoroutine();
            yield return WaitCoroutine(_animatorCoroutine, null, failure, false);
            
            yield return base.Progress(unit);
        }

        protected override IEnumerator Canceling(IAttachment unit)
        {
            if (_animatorCoroutine != null)
            {
                _animatorCancelCoroutine ??= unit.Animator.CancelToggle(unit, this).ToCoroutine();
                yield return WaitCoroutine(_animatorCancelCoroutine, null, null, false);
            }

            if (_controllerCoroutine != null)
            {
                _controllerCancelCoroutine ??= unit.Controller.CancelToggle(unit, this).ToCoroutine();
                yield return WaitCoroutine(_controllerCancelCoroutine, null, null, false);
            }

            yield return base.Canceling(unit);
        }
    }
}