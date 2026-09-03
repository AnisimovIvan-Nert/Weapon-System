using System;
using System.Collections.Generic;
using System.Threading;

namespace Scratch.InteractionArchitecture
{
    /// <summary>
    /// Records every mutation an interaction performs so it can be
    /// rolled back atomically if cancelled at any stage.
    /// </summary>
    public sealed class Transaction
    {
        private readonly List<Action> _compensations = new();
        //TODO Change to enum
        private int _status; // 0 = active, 1 = committed, 2 = rolled back

        //TODO Inverse and rename
        public bool IsActive => Volatile.Read(ref _status) == 0;

        /// <summary>
        /// Registers a compensating action that will undo <paramref name="mutation"/>
        /// when the transaction is rolled back. The mutation itself is executed immediately.
        /// </summary>
        public T Apply<T>(Func<T> mutation, Func<T, Action> compensationFactory)
        {
            if (!IsActive)
                throw new InvalidOperationException("Transaction is no longer active.");

            T result = mutation();
            _compensations.Add(compensationFactory(result));
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
            
            _compensations.Add(compensation);
        }

        /// <summary>
        /// Commits the transaction — marks it done without undoing anything.
        /// </summary>
        public bool TryCommit()
        {
            return Interlocked.CompareExchange(ref _status, 1, 0) == 0;
        }

        /// <summary>
        /// Rolls back all recorded mutations in reverse order.
        /// </summary>
        public bool TryRollback()
        {
            if (Interlocked.CompareExchange(ref _status, 2, 0) != 0)
                return false;
            
            //TODO Make _compensations a Stack
            for (var i = _compensations.Count - 1; i >= 0; i--)
            {
                try
                {
                    _compensations[i]();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }

            _compensations.Clear();
            return true;
        }
    }
}
