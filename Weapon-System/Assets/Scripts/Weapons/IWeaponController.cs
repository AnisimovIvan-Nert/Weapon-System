using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons
{
    public interface IWeaponController
    {
        void Update(IWeapon weapon);

        Task PerformShot(IWeapon weapon, ShotOperation operation);
        Task CancelShot(IWeapon weapon, ShotOperation operation);
    }

    public class WeaponController : IWeaponController
    {
        public const string Start = "Control Start Shot";
        public const string Perform = "Control Perform Shot";
        public const string Cancel = "Control Cancel Shot";
        
        public void Update(IWeapon weapon)
        {
        }

        public async Task PerformShot(IWeapon weapon, ShotOperation operation)
        {
            Debug.Log(Start);
            
            var cancel = weapon.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
            
            await Task.Yield();
            Debug.Log(Perform);
            
            cancel = weapon.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
        }

        public async Task CancelShot(IWeapon weapon, ShotOperation operation)
        {
            await Task.Yield();
            Debug.Log(Cancel);
        }
    }
}