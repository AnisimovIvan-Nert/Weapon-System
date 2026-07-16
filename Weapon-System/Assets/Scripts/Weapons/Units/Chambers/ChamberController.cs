using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Chambers
{
    public interface IChamberController : IUnitController
    {
        Task PerformShot(IChamber unit, IOperation<IChamber> operation);
        Task CancelShot(IChamber unit, IOperation<IChamber> operation);
    }
    
    public class ChamberController : IChamberController
    {
        public const string Start = "Control Start Shot";
        public const string Perform = "Control Perform Shot";
        public const string Cancel = "Control Cancel Shot";
        
        public void Update(IUnit unit)
        {
        }

        public async Task PerformShot(IChamber unit, IOperation<IChamber> operation)
        {
            Debug.Log(Start);
            
            var cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
            
            await Task.Yield();
            Debug.Log(Perform);
            
            cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
        }

        public async Task CancelShot(IChamber unit, IOperation<IChamber> operation)
        {
            await Task.Yield();
            Debug.Log(Cancel);
        }
    }
}