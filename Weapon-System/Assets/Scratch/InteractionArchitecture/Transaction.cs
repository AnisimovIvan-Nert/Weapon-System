using System;
using System.Collections.Generic;
using System.Threading;

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
    /// rolled back atomically if cancelled at any stage.
    /// </summary>
    public sealed class Transaction
    {
        private readonly Stack<Action> _compensations = new();
        
        private int _status; // raw TransactionStatus value, CAS-friendly
        public TransactionStatus Status => (TransactionStatus)Volatile.Read(ref _status);

        /// <summary>True while the transaction can still be committed or rolled back.</summary>
        public bool IsActive => Status == TransactionStatus.Active;

        /// <summary>
        /// Registers a compensating action that will undo <paramref name="mutation"/>
        /// when the transaction is rolled back. The mutation itself is executed immediately.
        /// </summary>
        public T Apply<T>(Func<T> mutation, Func<T, Action> compensationFactory)
        {
            if (!IsActive)
                throw new InvalidOperationException("Transaction is no longer active.");

            T result = mutation();
            _compensations.Push(compensationFactory(result));
            return result;
        }

        /// <summary>
        /// Registers only a compensating action without executing anything.
        /// Use when the mutation was already performed externally.
        /// </summary>
        public void RegisterCompensation(Action compensation)
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
        /// Rolls back all recorded mutations in reverse order.
        /// </summary>
        public bool TryRollback()
        {
            const int rolledBack = (int)TransactionStatus.RolledBack;
            const int active = (int)TransactionStatus.Active;
            if (Interlocked.CompareExchange(ref _status, rolledBack, active) != active)
            {
                return false;
            }

            while (_compensations.Count > 0)
            {
                try
                {
                    _compensations.Pop()();
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
