namespace Weapons.Units.Attachments
{
    public interface IAttachment : IUnit<IAttachment, IUnitData, IAttachmentController, IAttachmentAnimator>
    {
        int AttachmentNumber { get; set; }
    }
}