using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture
{
    /// <summary>Lifecycle state of an interaction.</summary>
    public enum InteractionState
    {
        Pending,
        Running,
        Committed,
        RolledBack,
        Failed
    }

    /// <summary>
    /// An interaction is an ordered pipeline of <see cref="InteractionStage"/>s
    /// executed inside a <see cref="Transaction"/> with a shared typed context.
    /// It can be cancelled at any moment — every mutation is automatically undone
    /// via compensations.
    ///
    /// Stages run sequentially on the scheduler's thread (typically Unity main
    /// thread). To reach objects that live on other threads, use
    /// <see cref="InteractionWorld.Dispatcher"/> to marshal calls onto them.
    /// </summary>
    public sealed class Interaction<TContext> : IInteraction, IDisposable
        where TContext : class, new()
    {
        private readonly IReadOnlyList<InteractionStage<TContext>> _stages;
        private readonly Transaction _transaction = new();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private int _currentStage;

        public Guid Id { get; } = Guid.NewGuid();
        public TContext Context { get; } = new();
        public InteractionState State { get; private set; }
        

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;
        public bool IsCompleted => State is InteractionState.Committed
                                          or InteractionState.RolledBack
                                          or InteractionState.Failed;

        internal Interaction(IEnumerable<InteractionStage<TContext>> stages, CancellationToken externalToken)
        {
            _stages = new List<InteractionStage<TContext>>(stages);
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            State = InteractionState.Pending;
        }
        
        public void Dispose()
        {
            if (!IsCompleted)
                Cancel();
            _cancellationTokenSource.Dispose();
        }

        /// <summary>
        /// Runs every stage in order.  Each stage receives a <c>yield</c>
        /// callback that suspends execution until the next frame, so heavy
        /// work can be distributed across frames.
        /// </summary>
        internal Task RunAsync(Func<Task> yield)
        {
            return ExecuteStageChainAsync(yield);
        }

        Task IInteraction.AdvanceAsync(Func<Task> yield) => RunAsync(yield);
        void IInteraction.Cancel() => Cancel();

        private async Task ExecuteStageChainAsync(Func<Task> yield)
        {
            State = InteractionState.Running;

            try
            {
                for (; _currentStage < _stages.Count; _currentStage++)
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    var stage = _stages[_currentStage];
                    await stage.ExecuteAsync(_transaction, Context, yield, _cancellationTokenSource.Token)
                              .ConfigureAwait(false);
                }

                if (_transaction.TryCommit())
                {
                    State = InteractionState.Committed;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Interaction {Id} failed to commit: transaction already finalized.");
                }
            }
            catch (OperationCanceledException)
            {
                _transaction.TryRollback();
                State = InteractionState.RolledBack;
            }
            catch (Exception)
            {
                _transaction.TryRollback();
                State = InteractionState.Failed;
                throw;
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
