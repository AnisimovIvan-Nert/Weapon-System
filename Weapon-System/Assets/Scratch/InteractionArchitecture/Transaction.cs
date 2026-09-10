using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture
{
    /// <summary>Lifecycle status of a <see cref="Transaction"/>.</summary>
    /// <remarks>
    /// <see cref="Active"/> is defined as 0 so it is the natural default value
    /// of a freshly-created transaction, and so a volatile read initially sees
    /// it as active without explicit initialisation.
    /// </remarks>
    public enum TransactionStatus
    {
        Active = 0,
        Committed = 1,
        RolledBack = 2
    }

    /// <summary>
    /// Records every mutation an interaction performs so it can be
    /// rolled back atomically if cancelled at any stage. Mutations that
    /// marshal onto another thread (e.g. via
    /// <see cref="OwnedObject.RunOnOwnerAsync{T}(Func{Task{T}})"/>) are
    /// recorded with the async <see cref="ApplyAsync{T}(Func{Task{T}}, Func{T, Task})"/>.
    /// </summary>
    public sealed class Transaction
    {
        private readonly Stack<Func<Task>> _compensations = new();

        private int _status; // raw TransactionStatus value, CAS-friendly
        public TransactionStatus Status => (TransactionStatus)Volatile.Read(ref _status);

        /// <summary>True while the transaction can still be committed or rolled back.</summary>
        public bool IsActive => Status == TransactionStatus.Active;

        /// <summary>
        /// Registers a compensating action that will undo <paramref name="mutation"/>
        /// when the transaction is rolled back. The mutation itself is executed immediately.
        /// Use for pure, synchronous mutations that need no thread marshalling.
        /// </summary>
        public T Apply<T>(Func<T> mutation, Func<T, Action> compensationFactory)
        {
            if (!IsActive)
                throw new InvalidOperationException("Transaction is no longer active.");

            T result = mutation();
            _compensations.Push(() =>
            {
                compensationFactory(result)();
                return Task.CompletedTask;
            });
            return result;
        }

        /// <summary>
        /// Asynchronous variant of <see cref="Apply{T}"/>: the mutation returns a
        /// <see cref="Task"/> (typically a marshalled <see cref="OwnedObject.RunOnOwnerAsync{T}(Func{Task{T}})"/>
        /// call) and is awaited before the compensation is recorded, so the
        /// compensation only sees the committed result. Use for mutations that
        /// cross thread boundaries.
        /// </summary>
        public async Task<T> ApplyAsync<T>(Func<Task<T>> mutation, Func<T, Task> compensationFactory)
        {
            if (!IsActive)
                throw new InvalidOperationException("Transaction is no longer active.");

            T result = await mutation();
            _compensations.Push(() => compensationFactory(result));
            return result;
        }

        /// <summary>
        /// Registers only a synchronous compensating action without executing anything.
        /// Use when the mutation was already performed externally.
        /// </summary>
        public void RegisterCompensation(Action compensation)
        {
            if (!IsActive)
                throw new InvalidOperationException("Transaction is no longer active.");

            _compensations.Push(() =>
            {
                compensation();
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Registers only an asynchronous compensating action (e.g. one that
        /// marshals the undo onto another thread) without executing anything.
        /// </summary>
        public void RegisterCompensation(Func<Task> compensation)
        {
            if (!IsActive)
                throw new InvalidOperationException("Transaction is no longer active.");

            _compensations.Push(compensation);
        }

        /// <summary>
        /// Commits the transaction — marks it done without undoing anything.
        /// </summary>
        public bool TryCommit()
        {
            const int committed = (int)TransactionStatus.Committed;
            const int active = (int)TransactionStatus.Active;
            return Interlocked.CompareExchange(ref _status, committed, active) == active;
        }

        /// <summary>
        /// Rolls back all recorded mutations in reverse order, awaiting each
        /// compensation so cross-thread undos complete before this returns.
        /// </summary>
        public Task<bool> TryRollbackAsync()
        {
            const int rolledBack = (int)TransactionStatus.RolledBack;
            const int active = (int)TransactionStatus.Active;
            if (Interlocked.CompareExchange(ref _status, rolledBack, active) != active)
                return Task.FromResult(false);

            return RollbackCoreAsync();
        }

        private async Task<bool> RollbackCoreAsync()
        {
            while (_compensations.Count > 0)
            {
                try
                {
                    await _compensations.Pop()();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }

            return true;
        }
    }
}