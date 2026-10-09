using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Operations
{
    public sealed class OperationHandler<TContext> : IOperationHandler
        where TContext : IOperationContext
    {
        private readonly IOperationMiddlewareSource _middlewareSource;
        private readonly TContext _context;
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

        public OperationHandler(
            IOperationMiddlewareSource middlewareSource,
            TContext context,
            params IOperation<TContext>[] operations)
        {
            _middlewareSource = middlewareSource;
            _context = context;
            _operations = operations.ToArray();
            _cts = new CancellationTokenSource();

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

            var middlewares = _middlewareSource.GetMiddlewares().ToArray();
            var executedMiddlewares = new Stack<IOperationMiddleware>();

            try
            {
                foreach (var operation in _operations)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    await ExecuteOperation(operation, transaction, middlewares, executedMiddlewares);
                }

                Commit(transaction);
            }
            catch (OperationEnforceComplete)
            {
                Commit(transaction);
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

        private async ValueTask ExecuteOperation(
            IOperation<TContext> operation,
            Transaction transaction,
            IOperationMiddleware[] middlewares,
            Stack<IOperationMiddleware> executedMiddlewares)
        {
            executedMiddlewares.Clear();

            try
            {
                foreach (var middleware in middlewares)
                {
                    var executed = await middleware.BeforeExecution(operation, transaction, _context, _cts.Token);
                    if (executed)
                        executedMiddlewares.Push(middleware);
                }

                await operation.Execute(transaction, _context, _cts.Token).ConfigureAwait(false);
                
                while (executedMiddlewares.TryPop(out var middleware))
                    await middleware.AfterExecution(operation, transaction, _context, _cts.Token);
            }
            catch (OperationEnforceComplete e)
            {
                if (e.Cascade)
                    throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                var handled = false;
                foreach (var middleware in middlewares)
                {
                    handled = await TryHandle(middleware, e, operation, transaction);
                    if (handled)
                        break;
                }

                if (!handled)
                    throw;
            }
        }

        private async ValueTask<bool> TryHandle(IOperationMiddleware middleware, 
            Exception exception, 
            IOperation<TContext> operation,
            Transaction transaction)
        {
            try
            {
                return await middleware.TryHandleException(operation, exception, transaction, _context);
            }
            catch (OperationEnforceComplete enforce)
            {
                if (enforce.Cascade)
                    throw;

                return true;
            }
        }

        private void Commit(Transaction transaction)
        {
            if (!transaction.TryCommit())
                throw new InvalidOperationException($"Operation {Id} failed to commit");

            State = OperationState.Committed;
        }
    }
}