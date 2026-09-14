using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.Owning
{
    public class ObjectOwnerHandle : IObjectOwnerHandle
    {
        private volatile IObjectOwner _owner;

        //Upon ChangeOwner wait for current requests to drain and buffer new ones
        private bool _draining;
        private readonly ConcurrentQueue<Action> _drainBuffer = new();
        private readonly ConcurrentBag<Task> _pendingRequests = new();
        private readonly ReaderWriterLockSlim _drainLock = new();

        public ObjectOwnerHandle(IObjectOwner owner)
        {
            _owner = owner;
        }

        public ValueTask RunOnOwner(Action action)
        {
            if (_owner.TryRunImmediately(action))
                return new ValueTask(Task.CompletedTask);

            _drainLock.EnterReadLock();
            try
            {
                if (_draining)
                {
                    var (wrappedAction, task) = action.WrapWithTask();
                    _drainBuffer.Enqueue(wrappedAction);
                    return new ValueTask(task);
                }

                var requestTask = _owner.RunOnOwner(action);

                if (!requestTask.IsCompleted)
                    _pendingRequests.Add(requestTask.AsTask());

                return requestTask;
            }
            finally
            {
                _drainLock.ExitReadLock();
            }
        }

        public ValueTask<T> RunOnOwner<T>(Func<T> func)
        {
            if (_owner.TryRunImmediately(func, out var result))
                return new ValueTask<T>(result);

            _drainLock.EnterReadLock();
            try
            {
                if (_draining)
                {
                    var (wrappedAction, task) = func.WrapWithTask();
                    _drainBuffer.Enqueue(wrappedAction);
                    return new ValueTask<T>(task);
                }

                var requestTask = _owner.RunOnOwner(func);

                if (!requestTask.IsCompleted)
                    _pendingRequests.Add(requestTask.AsTask());

                return requestTask;
            }
            finally
            {
                _drainLock.ExitReadLock();
            }
        }

        public bool TryRunImmediately(Action action) => _owner.TryRunImmediately(action);
        public bool TryRunImmediately<T>(Func<T> func, out T result) => _owner.TryRunImmediately(func, out result);
        public void Enqueue(Action action) => _owner.Enqueue(action);

        public async ValueTask ChangeOwner(IObjectOwner owner)
        {
            _drainLock.EnterWriteLock();
            _draining = true;
            _drainLock.ExitWriteLock();

            await Task.WhenAll(_pendingRequests.ToArray());
            _pendingRequests.Clear();

            _drainLock.EnterWriteLock();
            try
            {
                _owner = owner;
                _draining = false;

                while (_drainBuffer.TryDequeue(out var action))
                    _owner.Enqueue(action);
            }
            finally
            {
                _drainLock.ExitWriteLock();
            }
        }
    }
}