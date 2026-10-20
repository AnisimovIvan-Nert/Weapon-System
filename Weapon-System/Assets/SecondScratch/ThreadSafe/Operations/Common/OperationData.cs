using System.Threading;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public readonly struct OperationData
    {
        public readonly Transaction Transaction;
        public readonly CancellationTokenSource Cts;

        public OperationData(Transaction transaction, CancellationTokenSource cts)
        {
            Transaction = transaction;
            Cts = cts;
        }
    }
}