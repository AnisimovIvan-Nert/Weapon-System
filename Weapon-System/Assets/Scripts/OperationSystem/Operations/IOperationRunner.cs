namespace OperationSystem.Operations
{
    public interface IOperationRunner
    {
        void Update();
        void RunOperation(IOperation operation, IOperationContext context);
    }
}