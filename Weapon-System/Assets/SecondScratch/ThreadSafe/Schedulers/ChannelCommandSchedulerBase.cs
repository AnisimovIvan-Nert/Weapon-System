using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Schedulers
{
    public abstract class ChannelCommandSchedulerBase<TCommand> : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly Channel<TCommand>[] _channels;
        private readonly int[] _pendingCounts;

        private Task[]? _consumerTasks;
        private readonly object _consumerTasksLock = new();

        public int ChannelCount => _channels.Length;

        protected ChannelCommandSchedulerBase(int channelCount)
        {
            if (channelCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(channelCount));

            _channels = new Channel<TCommand>[channelCount];
            _pendingCounts = new int[channelCount];

            // ReSharper disable once VirtualMemberCallInConstructor
            for (var i = 0; i < channelCount; i++)
                _channels[i] = CreateChannel();
        }
        
        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            foreach (var channel in _channels)
                channel.Writer.TryComplete();

            if (_consumerTasks != null)
                await Task.WhenAll(_consumerTasks);

            _cts.Dispose();
        }

        public int GetTotalPendingCommands()
        {
            var total = 0;
            for (var i = 0; i < _pendingCounts.Length; i++)
                total += PendingCommandCount(i);
            return total;
        }

        public int PendingCommandCount(int channelIndex) => Volatile.Read(ref _pendingCounts[channelIndex]);

        public void RunConsumers()
        {
            lock (_consumerTasksLock)
            {
                if (_consumerTasks != null)
                    throw new InvalidOperationException();

                _consumerTasks = new Task[_channels.Length];

                for (var i = 0; i < _channels.Length; i++)
                {
                    var channelIndex = i;
                    _consumerTasks[i] = Task.Run(() => ConsumeChannel(channelIndex));
                }
            }
        }

        public async Task DrainAsync()
        {
            var drainTasks = new Task[_channels.Length];
            for (var i = 0; i < _channels.Length; i++)
                drainTasks[i] = DrainChannelAsync(i);
            await Task.WhenAll(drainTasks);
        }

        protected async Task DrainChannelAsync(int channelIndex)
        {
            var reader = _channels[channelIndex].Reader;

            while (Volatile.Read(ref _pendingCounts[channelIndex]) > 0)
            {
                while (reader.TryRead(out var item))
                    ExecuteAndTrack(item, channelIndex);

                if (Volatile.Read(ref _pendingCounts[channelIndex]) > 0)
                    await Task.Yield();
            }
        }

        protected abstract void ExecuteCommand(TCommand command);

        protected virtual Channel<TCommand> CreateChannel() => Channel.CreateUnbounded<TCommand>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = true,
            });

        protected virtual void OnCommandExecuted(TCommand command)
        {
        }

        protected bool TryScheduleToChannel(int channelIndex, TCommand command)
        {
            Interlocked.Increment(ref _pendingCounts[channelIndex]);

            if (_channels[channelIndex].Writer.TryWrite(command))
                return true;

            Interlocked.Decrement(ref _pendingCounts[channelIndex]);
            return false;
        }

        protected int FindLeastPendingChannel()
        {
            var result = 0;
            var leastPending = int.MaxValue;

            for (var i = 0; i < _pendingCounts.Length; i++)
            {
                var pending = Volatile.Read(ref _pendingCounts[i]);
                if (pending >= leastPending)
                    continue;

                leastPending = pending;
                result = i;
                if (pending == 0)
                    break;
            }

            return result;
        }

        private async Task ConsumeChannel(int channelIndex)
        {
            try
            {
                var reader = _channels[channelIndex].Reader;

                while (await reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
                {
                    while (reader.TryRead(out var item))
                        ExecuteAndTrack(item, channelIndex);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void ExecuteAndTrack(TCommand item, int channelIndex)
        {
            try
            {
                ExecuteCommand(item);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Interlocked.Decrement(ref _pendingCounts[channelIndex]);
                OnCommandExecuted(item);
            }
        }
    }
}