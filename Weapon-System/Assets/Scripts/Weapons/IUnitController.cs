using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons
{
    public interface IUnitController
    {
        void Update(IUnit unit);
    }
    
    public interface IWeaponController : IUnitController
    {
        Task PerformShot(IWeapon unit, IOperation<IWeapon> operation);
        Task CancelShot(IWeapon unit, IOperation<IWeapon> operation);
    }
    
    public interface IAttachmentController : IUnitController
    {
        Task PerformToggle(IAttachment unit, IOperation<IAttachment> operation);
        Task CancelToggle(IAttachment unit, IOperation<IAttachment> operation);
    }

    public class WeaponController : IWeaponController
    {
        public const string Start = "Control Start Shot";
        public const string Perform = "Control Perform Shot";
        public const string Cancel = "Control Cancel Shot";
        
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
    
    public class AttachmentController : IAttachmentController
    {
        public const string Start = "Control Start Toggle";
        public const string Perform = "Control Perform Toggle";
        public const string Cancel = "Control Cancel Toggle";
        
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