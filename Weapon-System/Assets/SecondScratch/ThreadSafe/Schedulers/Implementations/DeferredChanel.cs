using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Schedulers.Implementations
{
    public delegate void HandleItem<in T>(T item);

    public class ChanelCompletedException : Exception
    {
    }

    public class DeferredChanel<T> : IDisposable
    {
        private readonly Channel<T> _channel;

        private readonly HandleItem<T> _handleItem;

        private int _pendingCount;

        public int PendingCount => _pendingCount;

        public DeferredChanel(
            Channel<T> channel,
            HandleItem<T> handleItem)
        {
            _channel = channel;
            _handleItem = handleItem;
        }

        public void Dispose()
        {
            _channel.Writer.TryComplete();
        }

        public void WriteToChannel(T value, bool incrementPending = true)
        {
            if (_channel.Writer.TryWrite(value))
            {
                if (incrementPending)
                    IncrementPending();
            }
            else
                throw new ChanelCompletedException();
        }

        public void IncrementPending()
        {
            Interlocked.Increment(ref _pendingCount);
        }

        public async ValueTask ConsumeChannel(CancellationTokenSource cts)
        {
            try
            {
                while (await _channel.Reader.WaitToReadAsync(cts.Token).ConfigureAwait(false))
                    ReadValues();
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
            while (_channel.Reader.TryRead(out var item))
            {
                _handleItem(item);
                Interlocked.Decrement(ref _pendingCount);
            }
        }
    }
}