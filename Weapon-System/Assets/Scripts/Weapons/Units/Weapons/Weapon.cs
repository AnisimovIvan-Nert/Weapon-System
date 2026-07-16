using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons.Units.Weapons
{
    public class Weapon : IWeapon
    {
        public IUserAdapter User { get; }
        public IUnitData Data { get; }
        public IWeaponController Controller { get; }
        public IWeaponAnimator Animator { get; }
        
        public IEnumerable<IOperationsRunner<IWeapon>> OperationsRunners { get; }

        public Weapon(
            IUserAdapter user,
            IUnitData data, 
            IWeaponController controller,
            IWeaponAnimator animator,
            IEnumerable<IOperationsRunner<IWeapon>> operationsRunners)
        {
            User = user;
            Data = data;
            Controller = controller;
            Animator = animator;
            OperationsRunners = operationsRunners;
        }
        
        public void Update()
        {
            User.Update();

            foreach (var operationsRunner in OperationsRunners)
                operationsRunner.Update(this);

            Controller.Update(this);
            Animator.Update(this);
        }
    }
}