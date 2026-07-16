using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Magazines
{
    public interface IMagazineController : IUnitController
    {
        IEnumerator PerformToggle(IMagazine unit, IOperation<IMagazine> operation);
        IEnumerator CancelToggle(IMagazine unit, IOperation<IMagazine> operation);
    }
    
    public class MagazineController : IMagazineController
    {
        public const string Start = "Control Start Toggle";
        public const string Perform = "Control Perform Toggle";
        public const string Cancel = "Control Cancel Toggle";
        
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