using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations
{
    public enum InteractionState
    {
        Pending,
        Running,
        Committed,
        RolledBack,
        Failed
    }

    public sealed class Operation<TContext> : IOperation, IDisposable
        where TContext : class, new()
    {
        private readonly OperationStage<TContext>[] _stages;
        private readonly Transaction _transaction = new();
        private readonly CancellationTokenSource _cts;

        public Guid Id { get; } = Guid.NewGuid();
        public TContext Context { get; } = new();
        public InteractionState State { get; private set; }


        public CancellationToken CancellationToken => _cts.Token;

        public bool IsCompleted => State is InteractionState.Committed
            or InteractionState.RolledBack
            or InteractionState.Failed;

        public Operation(IEnumerable<OperationStage<TContext>> stages, CancellationToken externalToken)
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

        public async ValueTask Execute()
        {
            State = InteractionState.Running;

            try
            {
                foreach (var stage in _stages)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    await stage.Execute(_transaction, Context, _cts.Token).ConfigureAwait(false);
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