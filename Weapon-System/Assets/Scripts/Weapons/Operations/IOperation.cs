namespace Weapons.Operations
{
    public interface IOperation<in T>
        where T : IUnit
    {
        OperationState State { get; }
        OperationResult Result { get; }
        
        void Increment(T unit);
    }

    public enum OperationState
    {
        Pending,
        InProgress,
        InCancellation,
        Complete,
        ReadyForDestroying,
        Destroying
    }

    public enum OperationResult
    {
        None,
        Success,
        Failure
    }
}