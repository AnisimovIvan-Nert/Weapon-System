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
        private TaskCompletionSource<object?>? _endOfQueueTcs;

        private int _threadId = Thread.CurrentThread.ManagedThreadId;
        private readonly ReaderWriterLockSlim _threadIdLock = new();

        private TaskCompletionSource<object?>? _terminateTcs;
        private readonly ReaderWriterLockSlim _terminateLock = new();
        
        public void UpdateThreadId()
        {
            _threadIdLock.EnterWriteLock();
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _threadIdLock.ExitWriteLock();
        }

        public bool ExecuteNext()
        {
            if (!_queue.TryDequeue(out var action))
            {
                if (_terminateTcs == null)
                    return false;

                _terminateLock.EnterWriteLock();
                try
                {
                    _terminateTcs.SetResult(null);
                }
                finally
                {
                    _terminateLock.ExitWriteLock();
                }

                return false;
            }

            action();
            return true;
        }

        public ValueTask Terminate()
        {
            _terminateLock.EnterWriteLock();
            try
            {
                if (_queue.IsEmpty)
                    return new ValueTask(Task.CompletedTask);

                if (_terminateTcs != null)
                    return new ValueTask(_terminateTcs.Task);

                _terminateTcs = new TaskCompletionSource<object?>();
                return new ValueTask(_terminateTcs.Task);
            }
            finally
            {
                _terminateLock.ExitWriteLock();
            }
        }

        public ValueTask RunOnOwner(Action action)
        {
            _terminateLock.EnterReadLock();
            try
            {
                if (TryRunImmediately(action))
                    return new ValueTask(Task.CompletedTask);

                if (_terminateTcs != null)
                    throw new InvalidOperationException("Accepting of new work is terminated");

                var tcs = new TaskCompletionSource<object?>();
                _queue.Enqueue(() =>
                {
                    try
                    {
                        RunImmediately(action);
                        tcs.SetResult(null);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                });
                return new ValueTask(tcs.Task);
            }
            finally
            {
                _terminateLock.ExitReadLock();
            }
        }

        public ValueTask<T> RunOnOwner<T>(Func<T> func)
        {
            _terminateLock.EnterReadLock();
            try
            {
                if (TryRunImmediately(func, out var result))
                    return new ValueTask<T>(result);

                if (_terminateTcs != null)
                    throw new InvalidOperationException("Accepting of new work is terminated");

                var tcs = new TaskCompletionSource<T>();
                _queue.Enqueue(() =>
                {
                    try
                    {
                        var funcResult = RunImmediately(func);
                        tcs.SetResult(funcResult);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                });
                return new ValueTask<T>(tcs.Task);
            }
            finally
            {
                _terminateLock.ExitReadLock();
            }
        }

        private bool TryRunImmediately(Action action)
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

        private bool TryRunImmediately<T>(Func<T> func, out T result)
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