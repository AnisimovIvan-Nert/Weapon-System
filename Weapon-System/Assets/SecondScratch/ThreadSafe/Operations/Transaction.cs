using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Operations
{
    public delegate ValueTask Rollback();

    public delegate ValueTask StatefulRollback<in T>(T state);
    
    public enum TransactionStatus
    {
        Active = 0,
        Committed = 1,
        RolledBack = 2
    }

    public sealed class Transaction
    {
        private const int Commited = (int)TransactionStatus.Committed;
        private const int Active = (int)TransactionStatus.Active;
        private const int RolledBack = (int)TransactionStatus.RolledBack;

        private readonly Stack<Rollback> _rollbacks = new();

        private int _status;

        public TransactionStatus Status => (TransactionStatus)Volatile.Read(ref _status);
        public bool IsActive => Status == TransactionStatus.Active;

        public T Apply<T>(Func<T> mutation, StatefulRollback<T> rollback)
        {
            if (!IsActive)
                throw new InvalidOperationException("Transaction is no longer active.");

            var result = mutation();
            _rollbacks.Push(async () => await rollback(result));
            return result;
        }

        public void RegisterRollback(Rollback rollback)
        {
            if (!IsActive)
                throw new InvalidOperationException("Transaction is no longer active.");

            _rollbacks.Push(async () => await rollback());
        }

        public bool TryCommit()
        {
            return Interlocked.CompareExchange(ref _status, Commited, Active) == Active;
        }

        public async ValueTask<bool> TryRollbackAsync()
        {
            if (Interlocked.CompareExchange(ref _status, RolledBack, Active) != Active)
                return false;

            await CallRollbacks();
            return true;
        }

        private async ValueTask CallRollbacks()
        {
            while (_rollbacks.TryPop(out var rollback))
            {
                try
                {
                    await rollback();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}