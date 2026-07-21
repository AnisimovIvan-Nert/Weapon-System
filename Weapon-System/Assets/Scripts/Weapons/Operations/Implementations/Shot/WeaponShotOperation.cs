using System;
using System.Collections;
using Weapons.Units.Weapons;

namespace Weapons.Operations.Implementations.Shot
{
    public class WeaponShotOperation : AbstractOperation<IWeapon>
    {
        public WeaponShotOperation(Guid identifier, IWeapon unit) 
            : base(identifier, unit)
        {
        }

        protected override IEnumerator Progress()
        {
            
            
            return base.Progress();
        }
    }
}