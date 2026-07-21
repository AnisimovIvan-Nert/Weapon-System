using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Weapons.Operations;
using Weapons.ProducerConsumer.Producers;

namespace Weapons.Units.Chambers
{
    public interface IChamberController : IUnitController
    {
        IEnumerator PerformShot(IChamber unit, IOperation operation);
        IEnumerator CancelShot(IChamber unit, IOperation operation);
    }
    
    public class ChamberController : IChamberController
    {
        public const string Start = "Control Start Shot";
        public const string Perform = "Control Perform Shot";
        public const string Cancel = "Control Cancel Shot";
        
        public void Update(IUnit unit)
        {
        }

        public IEnumerator PerformShot(IChamber unit, IOperation operation)
        {
            Debug.Log(Start);
            
            var cancel = unit.EventProducer.EnumerateEvents().Any(e => e is CancellationEvent);
            if (cancel)
                throw new Exception();

            yield return null;
            Debug.Log(Perform);
            
            cancel = unit.EventProducer.EnumerateEvents().Any(e => e is CancellationEvent);
            if (cancel)
                throw new Exception();
        }

        public IEnumerator CancelShot(IChamber unit, IOperation operation)
        {
            yield return null;
            Debug.Log(Cancel);
        }
    }
}