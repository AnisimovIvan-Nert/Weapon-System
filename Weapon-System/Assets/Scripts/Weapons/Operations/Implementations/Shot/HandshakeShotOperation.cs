using System;
using System.Collections;
using Coroutine;
using Weapons.Units.Weapons;

namespace Weapons.Operations.Implementations.Shot
{
    public class HandshakeShotOperation : AbstractOperation<IWeapon>
    {
        public HandshakeShotOperation(Guid identifier, IWeapon unit) 
            : base(identifier, unit)
        {
        }

        protected override IEnumerator Progress()
        {
            var coroutine = Unit.Controller.ShotController.Handshake(Unit, this).ToCoroutine();
            yield return WaitCoroutine(coroutine, null, OperationStatus.InCancellation);
            yield return base.Progress();
        }

        protected override IEnumerator Canceling()
        {
            var coroutine = Unit.Controller.ShotController.CancelHandshake(Unit, this).ToCoroutine();
            yield return WaitCoroutine(coroutine);
            yield return base.Canceling();
        }
    }
}