using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.TaskBasedImplementation
{
    public enum InteractionState
    {
        Pending,
        Running,
        Committed,
        RolledBack,
        Failed
    }

    public sealed class Interaction<TContext> : IInteraction, IDisposable
        where TContext : class, new()
    {
        private readonly InteractionStage<TContext>[] _stages;
        private readonly Transaction _transaction = new();
        private readonly CancellationTokenSource _cts;
        private int _currentStage;

        public Guid Id { get; } = Guid.NewGuid();
        public TContext Context { get; } = new();
        public InteractionState State { get; private set; }


        public CancellationToken CancellationToken => _cts.Token;

        public bool IsCompleted => State is InteractionState.Committed
            or InteractionState.RolledBack
            or InteractionState.Failed;

        public Interaction(IEnumerable<InteractionStage<TContext>> stages, CancellationToken externalToken)
        {
            _stages = stages.ToArray();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            State = InteractionState.Pending;
        }

        public void Dispose()
        {
            if (!IsCompleted)
                Cancel();
            _cts.Dispose();
        }

        public void Cancel() => _cts.Cancel();

        public async ValueTask ExecuteAsync()
        {
            State = InteractionState.Running;

            try
            {
                for (; _currentStage < _stages.Length; _currentStage++)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    var stage = _stages[_currentStage];
                    await stage.ExecuteAsync(_transaction, Context, _cts.Token).ConfigureAwait(false);
                }

                if (_transaction.TryCommit())
                {
                    State = InteractionState.Committed;
                }
                else
                {
                    throw new InvalidOperationException($"Interaction {Id} failed to commit");
                }
            }
            catch (OperationCanceledException)
            {
                await _transaction.TryRollbackAsync();
                State = InteractionState.RolledBack;
            }
            catch (Exception)
            {
                await _transaction.TryRollbackAsync();
                State = InteractionState.Failed;
                throw;
            }
        }
    }
}