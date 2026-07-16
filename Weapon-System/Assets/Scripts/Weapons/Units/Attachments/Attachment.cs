using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons.Units.Attachments
{
    public interface IAttachment : IUnit<IAttachmentData, IAttachmentController, IAttachmentAnimator>
    {
        int AttachmentNumber { get; set; }
    }

    public class Attachment 
        : AbstractUnit<IAttachmentData, IAttachmentController, IAttachmentAnimator>
        , IAttachment
    {
        public int AttachmentNumber { get; set; }

        public Attachment(
            IUserAdapter user,
            IAttachmentData data,
            IAttachmentController controller,
            IAttachmentAnimator animator,
            IEnumerable<IOperationsRunner> operationsRunners)
            : base(user, data, controller, animator, operationsRunners)
        {
        }
    }
}