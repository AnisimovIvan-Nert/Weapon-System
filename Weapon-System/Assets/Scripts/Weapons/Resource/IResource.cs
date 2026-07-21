namespace Weapons.Resource
{
    public interface IResource
    {
        bool IsLocked { get; }
        
        bool IsBelongs(object owner);
        bool TryAcquire(object owner);
        void Release(object owner);
        void ForceRelease();
    }
}