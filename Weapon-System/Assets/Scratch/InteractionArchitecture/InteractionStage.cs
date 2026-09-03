using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture
{
    /// <summary>
    /// A single atomic step inside an interaction, typed against
    /// the interaction's shared <typeparamref name="TContext"/>.
    ///
    /// Implementations call <see cref="Transaction.Apply{T}"/> for every
    /// mutation so that cancellation automatically undoes the work.
    /// Use the <paramref name="yield"/> callback to suspend until the
    /// next frame, distributing work across frames.
    /// </summary>
    public abstract class InteractionStage<TContext> 
        where TContext : class
    {
        public abstract string Name { get; }

        /// <summary>
        /// Execute this stage.  Call <paramref name="yield"/> to suspend
        /// until the next scheduler tick.  Throwing (or cancellation)
        /// causes the whole interaction to be rolled back and failed.
        /// </summary>
        public abstract Task ExecuteAsync(
            Transaction transaction,
            TContext context,
            Func<Task> yield,
            CancellationToken cancellationToken);
    }
}
