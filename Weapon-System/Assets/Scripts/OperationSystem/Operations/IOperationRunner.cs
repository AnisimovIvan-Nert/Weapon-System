namespace OperationSystem.Operations
{
    public interface IOperationRunner
    {
        bool AnyRunningOperation { get; }
        
        void Update();
        void RunOperation(IOperation operation, IOperationContext context);

        bool TryLockOperationRunning(out ILock? @lock);
        void ReleaseOperationRunning(ILock @lock);

        public interface ILock
        {
        }

        internal class Lock : ILock
        {
        }
    }
}