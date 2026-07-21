using System;
using System.Collections;
using Weapons.Operations;

namespace Weapons.Units.Weapons.Controller
{
    public interface IWeaponShotOperationController : IOperationController
    {
    }
    
    public class WeaponShotOperationController : IWeaponShotOperationController
    {
        public IEnumerator Handshake(IUnit unit, IOperation operation)
        {
            if (unit is not IWeapon weapon)
                throw new InvalidOperationException();
        }

        public IEnumerator CancelHandshake(IUnit unit, IOperation operation)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator Perform(IUnit unit, IOperation operation)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator CancelPerform(IUnit unit, IOperation operation)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator OnSuccess(IUnit unit, IOperation operation)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator OnFailure(IUnit unit, IOperation operation)
        {
            throw new System.NotImplementedException();
        }
    }
}