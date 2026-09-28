using System.Threading;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations
{
    public abstract class OperationStage<TContext>
        where TContext : class
    {
        public abstract string Name { get; }

        public abstract ValueTask Execute(Transaction transaction, TContext context, CancellationToken ct);
    }
}