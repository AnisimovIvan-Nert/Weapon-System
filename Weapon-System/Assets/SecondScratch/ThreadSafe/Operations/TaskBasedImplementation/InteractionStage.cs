using System.Threading;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.TaskBasedImplementation
{
    public abstract class InteractionStage<TContext>
        where TContext : class
    {
        public abstract string Name { get; }

        public abstract ValueTask ExecuteAsync(Transaction transaction, TContext context, CancellationToken ct);
    }
}