using SecondScratch.Shared;

namespace SecondScratch.ThreadSafe.LockableObject
{
    public interface ILockableObject
    {
        ConditionalEvent OnReadyForLock { get; }
        
        bool TryLock();
        void Release();
    }
}