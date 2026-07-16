using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Magazines
{
    public interface IMagazineAnimator : IUnitAnimator
    {
        IEnumerator PerformToggle(IMagazine unit, IOperation<IMagazine> operation);
        IEnumerator CancelToggle(IMagazine unit, IOperation<IMagazine> operation);
    }
    
    public class MagazineAnimator : IMagazineAnimator
    {
        public const string Start = "Animate Start Toggle";
        public const string Perform = "Animate Perform Toggle";
        public const string Cancel = "Animate Cancel Toggle";
        
        public void Update(IUnit unit)
        {
        }

        public IEnumerator PerformToggle(IMagazine unit, IOperation<IMagazine> operation)
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

        public IEnumerator CancelToggle(IMagazine unit, IOperation<IMagazine> operation)
        {
            yield return null;
            Debug.Log(Cancel);
        }
    }
}