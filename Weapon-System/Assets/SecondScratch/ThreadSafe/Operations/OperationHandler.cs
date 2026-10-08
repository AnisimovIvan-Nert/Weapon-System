using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Operations
{
    public sealed class OperationHandler<TContext> : IOperationHandler
    {
        private TContext _context;
        private readonly IOperation<TContext>[] _operations;
        private readonly CancellationTokenSource _cts;
        

        public Guid Id { get; } = Guid.NewGuid();
        public OperationState State { get; private set; }

        public IEnumerable<IOperationSubject> Subjects { get; }
        public IEnumerable<OperationTypes> Types { get; }
        public IEnumerable<OperationMember> Members { get; }
        
        public bool IsCompleted => State is OperationState.Committed
            or OperationState.RolledBack
            or OperationState.Failed;

        public OperationHandler(TContext context, CancellationToken externalToken, params IOperation<TContext>[] operations)
        {
            _context = context;
            _operations = operations.ToArray();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            State = OperationState.Pending;

            Subjects = new HashSet<IOperationSubject>(operations.SelectMany(o => o.Subjects));
            Types = new HashSet<OperationTypes>(operations.SelectMany(o => o.Types));
            Members = new HashSet<OperationMember>(operations.SelectMany(o => o.Members));
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
            State = OperationState.Running;
            var transaction = new Transaction();

            try
            {
                foreach (var operation in _operations)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    await operation.Execute(ref transaction, ref _context, _cts.Token).ConfigureAwait(false);
                }

                if (transaction.TryCommit())
                {
                    State = OperationState.Committed;
                }
                else
                {
                    throw new InvalidOperationException($"Operation {Id} failed to commit");
                }
            }
            catch (OperationCanceledException)
            {
                await transaction.TryRollbackAsync();
                State = OperationState.RolledBack;
            }
            catch (Exception)
            {
                await transaction.TryRollbackAsync();
                State = OperationState.Failed;
                throw;
            }
        }
    }
}