using System.Collections.Generic;
using Weapons.Operations;
using Weapons.Units.Attachments;
using Weapons.User;

namespace Weapons.Assets
{
    public class Laser : IAsset
    {
        public IAttachmentData Data;
        public IAttachmentController Controller;
        public IAttachmentAnimator Animator;
        public List<IOperationsRunner> OperationsRunners = new();
        
        public List<IAsset> Children { get; } = new();
        
        public Laser(IAttachmentData data, IAttachmentController controller, IAttachmentAnimator animator)
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