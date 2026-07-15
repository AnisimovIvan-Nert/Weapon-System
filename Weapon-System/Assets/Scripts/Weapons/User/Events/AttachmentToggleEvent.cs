namespace Weapons.User.Events
{
    public readonly struct AttachmentToggleEvent : IUserEvent
    {
        public int[] Attachments { get; }
        public bool All => Attachments.Length == 0;
        
        public AttachmentToggleEvent(int[] attachments)
        {
            Attachments = attachments;
        }
    }
}