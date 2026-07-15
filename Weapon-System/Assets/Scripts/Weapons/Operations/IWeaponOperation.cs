namespace Weapons.Operations
{
    public interface IWeaponOperation
    {
        OperationState State { get; }
        OperationResult Result { get; }
        
        void Increment(IWeapon weapon);
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