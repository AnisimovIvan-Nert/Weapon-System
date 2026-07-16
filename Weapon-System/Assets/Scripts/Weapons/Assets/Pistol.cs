using System.Collections.Generic;
using Weapons.Operations;
using Weapons.Units.Weapons;
using Weapons.User;

namespace Weapons.Assets
{
    public class Pistol : IAsset
    {
        public IWeaponData Data;
        public IWeaponController Controller;
        public IWeaponAnimator Animator;
        public List<IOperationsRunner<IWeapon>> OperationsRunners = new();

        public List<IAsset> Children { get; } = new();

        public Pistol(IWeaponData data, IWeaponController controller, IWeaponAnimator animator)
        {
            Data = data;
            Controller = controller;
            Animator = animator;
        }
        
        public IUnit ToUnit(IUserAdapter userAdapter)
        {
            return new Weapon(userAdapter, Data, Controller, Animator, OperationsRunners);
        }
    }
}