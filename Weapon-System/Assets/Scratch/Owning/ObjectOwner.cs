using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.Owning
{
    public class ObjectOwner : IObjectOwner
    {
        private class InvalidThreadException : Exception
        {
            public InvalidThreadException(int exceptedTheadId)
                : base($"Owner can execute only from Thread:{exceptedTheadId}," +
                       $" but was executed from Thread:{Thread.CurrentThread.ManagedThreadId}")
            {
            }
        }

        private readonly ConcurrentQueue<Action> _queue = new();

        private int _threadId = Thread.CurrentThread.ManagedThreadId;
        private readonly ReaderWriterLockSlim _threadIdLock = new(LockRecursionPolicy.SupportsRecursion);

        public void UpdateThreadId()
        {
            _threadIdLock.EnterWriteLock();
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _threadIdLock.ExitWriteLock();
        }

        public bool ExecuteNext()
        {
            if (!_queue.TryDequeue(out var action))
                return false;

            action();
            return true;
        }

        public ValueTask RunOnOwner(Action action)
        {
            if (TryRunImmediately(action))
                return new ValueTask(Task.CompletedTask);

            var (wrappedAction, task) = WrapAction().WrapWithTask();
            _queue.Enqueue(wrappedAction);
            return new ValueTask(task);

            Action WrapAction() => () => RunImmediately(action);
        }

        public ValueTask<T> RunOnOwner<T>(Func<T> func)
        {
            if (TryRunImmediately(func, out var result))
                return new ValueTask<T>(result);
            
            var (wrappedAction, task) = WrapAction().WrapWithTask();
            _queue.Enqueue(wrappedAction);
            return new ValueTask<T>(task);
            
            Func<T> WrapAction() => () => RunImmediately(func);
        }

        public bool TryRunImmediately(Action action)
        {
            try
            {
                RunImmediately(action);
                return true;
            }
            catch (InvalidThreadException)
            {
                return false;
            }
        }

        public bool TryRunImmediately<T>(Func<T> func, out T result)
        {
            result = default!;
            try
            {
                result = RunImmediately(func);
                return true;
            }
            catch (InvalidThreadException)
            {
                return false;
            }
        }

        public void Enqueue(Action action) => _queue.Enqueue(action);

        private void RunImmediately(Action action)
        {
            _threadIdLock.EnterReadLock();
            try
            {
                if (Thread.CurrentThread.ManagedThreadId != _threadId)
                    throw new InvalidThreadException(_threadId);

                action();
            }
            finally
            {
                _threadIdLock.ExitReadLock();
            }
        }

        private T RunImmediately<T>(Func<T> func)
        {
            _threadIdLock.EnterReadLock();
            try
            {
                if (Thread.CurrentThread.ManagedThreadId != _threadId)
                    throw new InvalidThreadException(_threadId);

                return func();
            }
            finally
            {
                _threadIdLock.ExitReadLock();
            }
        }
    }
}