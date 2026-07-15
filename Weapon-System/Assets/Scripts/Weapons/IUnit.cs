using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons
{
    public interface IUnit
    {
        IUserAdapter User { get; }
        
        void Update();
    }
    
    public interface IUnit<in T, out TData, out TController, out TAnimator> : IUnit
        where T : IUnit
        where TData : IUnitData
        where TController : IUnitController
        where TAnimator : IUnitAnimator
    {
        TData Data { get; }
        TController Controller { get; }
        TAnimator Animator { get; }
        
        IEnumerable<IOperationsRunner<T>> OperationsRunners { get; }
    }
    
    public interface IWeapon : IUnit<IWeapon, IUnitData, IWeaponController, IWeaponAnimator>
    {
    }
    
    public interface IAttachment : IUnit<IAttachment, IUnitData, IAttachmentController, IAttachmentAnimator>
    {
        int AttachmentNumber { get; set; }
    }
    
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
    
    public class Attachment : IAttachment
    {
        public int AttachmentNumber { get; set; }
        public IUserAdapter User { get; }
        public IUnitData Data { get; }
        public IAttachmentController Controller { get; }
        public IAttachmentAnimator Animator { get; }
        
        public IEnumerable<IOperationsRunner<IAttachment>> OperationsRunners { get; }

        public Attachment(
            IUserAdapter user,
            IUnitData data, 
            IAttachmentController controller,
            IAttachmentAnimator animator,
            IEnumerable<IOperationsRunner<IAttachment>> operationsRunners)
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