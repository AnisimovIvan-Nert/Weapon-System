using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons.Units.Attachments
{
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