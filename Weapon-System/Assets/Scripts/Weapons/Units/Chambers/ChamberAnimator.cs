using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Chambers
{
    public interface IChamberAnimator : IUnitAnimator
    {
        Task PerformShot(IChamber unit, IOperation<IChamber> operation);
        Task CancelShot(IChamber unit, IOperation<IChamber> operation);
    }
    
    public class ChamberAnimator : IChamberAnimator
    {
        public const string Start = "Animate Start Shot";
        public const string Perform = "Animate Perform Shot";
        public const string Cancel = "Animate Cancel Shot";
        
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