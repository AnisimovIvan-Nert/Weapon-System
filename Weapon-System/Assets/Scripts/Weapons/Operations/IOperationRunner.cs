namespace Weapons.Operations
{
    public interface IOperationRunner
    {
        public void Update();

        public void RunOperation(IOperation operation);
    }
}