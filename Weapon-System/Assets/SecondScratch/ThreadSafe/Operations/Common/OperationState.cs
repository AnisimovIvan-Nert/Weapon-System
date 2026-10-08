namespace SecondScratch.ThreadSafe.Operations.Common
{
    public enum OperationState
    {
        Pending,
        Running,
        Committed,
        RolledBack,
        Failed
    }
}