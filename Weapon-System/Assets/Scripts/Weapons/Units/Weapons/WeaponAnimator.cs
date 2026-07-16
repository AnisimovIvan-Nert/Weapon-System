using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Weapons
{
    public interface IWeaponAnimator : IUnitAnimator
    {
        IEnumerator PerformShot(IWeapon unit, IOperation<IWeapon> operation);
        IEnumerator CancelShot(IWeapon unit, IOperation<IWeapon> operation);
    }
    
    public class WeaponAnimator : IWeaponAnimator
    {
        public const string Start = "Animate Start Shot";
        public const string Perform = "Animate Perform Shot";
        public const string Cancel = "Animate Cancel Shot";
        
        public void Update(IUnit unit)
        {
        }

        public IEnumerator PerformShot(IWeapon unit, IOperation<IWeapon> operation)
        {
            Debug.Log(Start);
            
            var cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();

            yield return null;
            Debug.Log(Perform);
            
            cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
        }

        public IEnumerator CancelShot(IWeapon unit, IOperation<IWeapon> operation)
        {
            yield return null;
            Debug.Log(Cancel);
        }
    }
}