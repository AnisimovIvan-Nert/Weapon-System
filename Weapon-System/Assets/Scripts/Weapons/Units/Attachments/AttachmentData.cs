namespace Weapons.Units.Attachments
{
    public interface IAttachmentData : IUnitData
    {
        
    }
    
    public class AttachmentData : IAttachmentData
    {
        public string Name { get; }
        
        public AttachmentData(string name)
        {
            Name = name;
        }
    }
}