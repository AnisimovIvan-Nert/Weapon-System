using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons.Assets
{
    public interface IAsset
    {
        List<IAsset> Children { get; }

        IUnit ToUnit(IUserAdapter userAdapter);
    }
    
    public class Pistol : IAsset
    {
        public IUnitData Data;
        public IWeaponController Controller;
        public IWeaponAnimator Animator;
        public List<IOperationsRunner<IWeapon>> OperationsRunners = new();

        public List<IAsset> Children { get; } = new();

        public Pistol(IUnitData data, IWeaponController controller, IWeaponAnimator animator)
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

    public class Laser : IAsset
    {
        public IUnitData Data;
        public IAttachmentController Controller;
        public IAttachmentAnimator Animator;
        public List<IOperationsRunner<IAttachment>> OperationsRunners = new();
        
        public List<IAsset> Children { get; } = new();
        
        public Laser(IUnitData data, IAttachmentController controller, IAttachmentAnimator animator)
        {
            Data = data;
            Controller = controller;
            Animator = animator;
        }
        
        public IUnit ToUnit(IUserAdapter userAdapter)
        {
            return new Attachment(userAdapter, Data, Controller, Animator, OperationsRunners);
        }
    }
}