using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Magazines
{
    public interface IMagazineAnimator : IUnitAnimator
    {
        Task PerformToggle(IMagazine unit, IOperation<IMagazine> operation);
        Task CancelToggle(IMagazine unit, IOperation<IMagazine> operation);
    }
    
    public class MagazineAnimator : IMagazineAnimator
    {
        public const string Start = "Animate Start Toggle";
        public const string Perform = "Animate Perform Toggle";
        public const string Cancel = "Animate Cancel Toggle";
        
        public void Update(IUnit unit)
        {
        }

        public async Task PerformToggle(IMagazine unit, IOperation<IMagazine> operation)
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

        public async Task CancelToggle(IMagazine unit, IOperation<IMagazine> operation)
        {
            await Task.Yield();
            Debug.Log(Cancel);
        }
    }
}