using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons
{
    public interface IWeapon
    {
        IUserAdapter User { get; }
        IWeaponData Data { get; }
        IWeaponController Controller { get; }
        IWeaponAnimator Animator { get; }
        
        IEnumerable<IOperationsRunner> OperationsRunners { get; }
        
        void Update();
    }
    
    public class Weapon : IWeapon
    {
        public IUserAdapter User { get; }
        public IWeaponData Data { get; }
        public IWeaponController Controller { get; }
        public IWeaponAnimator Animator { get; }
        
        public IEnumerable<IOperationsRunner> OperationsRunners { get; }

        public Weapon(
            IUserAdapter user,
            IWeaponData data, 
            IWeaponController controller,
            IWeaponAnimator animator,
            IEnumerable<IOperationsRunner> operationsRunners)
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