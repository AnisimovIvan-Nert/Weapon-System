using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Magazines
{
    public interface IMagazineController : IUnitController
    {
        Task PerformToggle(IMagazine unit, IOperation<IMagazine> operation);
        Task CancelToggle(IMagazine unit, IOperation<IMagazine> operation);
    }
    
    public class MagazineController : IMagazineController
    {
        public const string Start = "Control Start Toggle";
        public const string Perform = "Control Perform Toggle";
        public const string Cancel = "Control Cancel Toggle";
        
        public void Update(IUnit unit)
        {
        }

        public async Task PerformToggle(IMagazine unit, IOperation<IMagazine> operation)
        {
            Debug.Log(Start);
            Debug.Log(unit.MagazineNumber);
            
            var cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
            
            await Task.Yield();
            Debug.Log(Perform);
            
            cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
        }

        public async Task CancelToggle(IMagazine unit, IOperation<IMagazine> operation)
        {
            await Task.Yield();
            Debug.Log(Cancel);
        }
    }
}