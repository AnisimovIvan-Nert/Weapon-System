namespace OperationSystem.Operations
{
    public interface IOperationRunner
    {
        bool AnyRunningOperation { get; }
        
        void Update();
        void RunOperation(IOperation operation, IOperationContext context);

        IDelayer DelayOperationRunning();
        void ReleaseOperationRunning(IDelayer delayer);

        public interface IDelayer
        {
        }

        internal class Delayer : IDelayer
        {
        }
    }
}