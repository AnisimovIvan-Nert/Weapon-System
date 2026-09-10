namespace Scratch.Owning
{
    public abstract class OwnedObject
    {
        public IObjectOwnerHandle Owner { get; private set; }
        
        protected OwnedObject(IObjectOwnerHandle owner)
        {
            Owner = owner;
        }
    }
}