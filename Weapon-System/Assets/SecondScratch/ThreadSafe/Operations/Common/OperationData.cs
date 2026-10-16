using System.Threading;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public readonly struct OperationData<TContext>
        where TContext : IOperationContext
    {
        public readonly Transaction Transaction;
        public TContext Context { get; }
        public readonly CancellationTokenSource Cts;

        public OperationData(Transaction transaction, TContext context, CancellationTokenSource cts)
        {
            Transaction = transaction;
            Context = context;
            Cts = cts;
        }
    }
}