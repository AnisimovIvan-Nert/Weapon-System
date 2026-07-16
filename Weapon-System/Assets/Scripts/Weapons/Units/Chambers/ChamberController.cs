using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Chambers
{
    public interface IChamberController : IUnitController
    {
        IEnumerator PerformShot(IChamber unit, IOperation<IChamber> operation);
        IEnumerator CancelShot(IChamber unit, IOperation<IChamber> operation);
    }
    
    public class ChamberController : IChamberController
    {
        public const string Start = "Control Start Shot";
        public const string Perform = "Control Perform Shot";
        public const string Cancel = "Control Cancel Shot";
        
        public void Update(IUnit unit)
        {
        }

        public IEnumerator PerformShot(IChamber unit, IOperation<IChamber> operation)
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

        public IEnumerator CancelShot(IChamber unit, IOperation<IChamber> operation)
        {
            yield return null;
            Debug.Log(Cancel);
        }
    }
}