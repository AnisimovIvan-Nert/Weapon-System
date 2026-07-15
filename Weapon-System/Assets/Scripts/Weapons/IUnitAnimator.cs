using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons
{
    public interface IUnitAnimator
    {
        void Update(IUnit unit);
    }
    
    public interface IWeaponAnimator : IUnitAnimator
    {
        Task PerformShot(IWeapon unit, IOperation<IWeapon> operation);
        Task CancelShot(IWeapon unit, IOperation<IWeapon> operation);
    }
    
    public interface IAttachmentAnimator : IUnitAnimator
    {
        Task PerformToggle(IAttachment unit, IOperation<IAttachment> operation);
        Task CancelToggle(IAttachment unit, IOperation<IAttachment> operation);
    }

    public class WeaponAnimator : IWeaponAnimator
    {
        public const string Start = "Animate Start Shot";
        public const string Perform = "Animate Perform Shot";
        public const string Cancel = "Animate Cancel Shot";
        
        public void Update(IUnit unit)
        {
        }

        public async Task PerformShot(IWeapon unit, IOperation<IWeapon> operation)
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

        public async Task CancelShot(IWeapon unit, IOperation<IWeapon> operation)
        {
            await Task.Yield();
            Debug.Log(Cancel);
        }
    }
    
    public class AttachmentAnimator : IAttachmentAnimator
    {
        public const string Start = "Animate Start Toggle";
        public const string Perform = "Animate Perform Toggle";
        public const string Cancel = "Animate Cancel Toggle";
        
        public void Update(IUnit unit)
        {
        }

        public async Task PerformToggle(IAttachment unit, IOperation<IAttachment> operation)
        {
            Debug.Log(Start);
            Debug.Log(unit.AttachmentNumber);
            
            var cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
            
            await Task.Yield();
            Debug.Log(Perform);
            
            cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
        }

        public async Task CancelToggle(IAttachment unit, IOperation<IAttachment> operation)
        {
            await Task.Yield();
            Debug.Log(Cancel);
        }
    }
}