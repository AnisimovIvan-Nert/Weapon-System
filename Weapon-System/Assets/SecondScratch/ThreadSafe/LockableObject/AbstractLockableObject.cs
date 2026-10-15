using System.Threading;
using SecondScratch.Shared;

namespace SecondScratch.ThreadSafe.LockableObject
{
    public abstract class AbstractLockableObject : ILockableObject
    {
        public ConditionalEvent OnReadyForLock { get; }
        
        protected AbstractLockableObject()
        {
            OnReadyForLock = new ConditionalEvent(TryLock);
        }
        
        public bool TryLock()
        {
            return Monitor.TryEnter(this);
        }

        public void Release()
        {
            Monitor.Exit(this);
            OnReadyForLock.Invoke();
        }
    }
}