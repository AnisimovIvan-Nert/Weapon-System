using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SecondScratch.Shared;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Schedulers.Implementations
{
    public delegate bool HandleItem<in T>(T item);

    public delegate void SubscribeOnStateChange<in T>(T item, Action action);

    public class DeferredChanel<T> : IDisposable
    {
        private readonly Channel<T> _channel;

        private readonly List<T> _deferred = new();
        private readonly object _semaphoreLock = new();
        private readonly SemaphoreSlim _stateChangeSemaphore = new(0, 1);

        private readonly HandleItem<T> _handleItem;
        private readonly SubscribeOnStateChange<T> _subscribeOnStateChange;

        private int _pendingCount;

        public int PendingCount => _pendingCount;

        public DeferredChanel(
            Channel<T> channel,
            HandleItem<T> handleItem,
            SubscribeOnStateChange<T> subscribeOnStateChange)
        {
            _channel = channel;
            _handleItem = handleItem;
            _subscribeOnStateChange = subscribeOnStateChange;
        }

        public void Dispose()
        {
            _channel.Writer.TryComplete();
            _stateChangeSemaphore.Dispose();
        }

        public bool TryWriteToChannel(T value)
        {
            var success = _channel.Writer.TryWrite(value);
            if (success)
                Interlocked.Increment(ref _pendingCount);
            return success;
        }

        public async ValueTask ConsumeChannel(CancellationTokenSource cts)
        {
            try
            {
                var waitTasks = new ValueTask<bool>[2];
                do
                {
                    ReadValues();

                    waitTasks[0] = _channel.Reader.WaitToReadAsync(cts.Token);
                    waitTasks[1] = WaitForStateChange(cts);
                } while (await waitTasks.WhenAny().ConfigureAwait(false));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void ReadValues()
        {
            for (var i = 0; i < _deferred.Count; i++)
            {
                var item = _deferred[i];
                if (!_handleItem(item))
                    continue;

                Interlocked.Decrement(ref _pendingCount);
                _deferred.RemoveAt(i);
                i--;
            }

            while (_channel.Reader.TryRead(out var item))
            {
                var handled = _handleItem(item);
                if (handled)
                {
                    Interlocked.Decrement(ref _pendingCount);
                    continue;
                }

                _deferred.Add(item);
                _subscribeOnStateChange(item, ReleaseStateChange);
            }
        }

        private void ReleaseStateChange()
        {
            if (_stateChangeSemaphore.CurrentCount != 0)
                return;

            lock (_semaphoreLock)
            {
                if (_stateChangeSemaphore.CurrentCount != 0)
                    return;

                _stateChangeSemaphore.Release();
            }
        }

        private async ValueTask<bool> WaitForStateChange(CancellationTokenSource cts)
        {
            await _stateChangeSemaphore.WaitAsync(cts.Token);
            return true;
        }
    }
}