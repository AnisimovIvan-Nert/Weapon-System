using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Tests.Mocks
{
    public class MultiChannelCommandScheduler : IAsyncDisposable
    {
        private readonly Channel<Entry>[] _channels;
        private readonly int[] _pendingCounts;
        private readonly CancellationTokenSource _cts = new();
        private readonly ConcurrentDictionary<object, TargetState> _targetStates = new();

        private readonly struct Entry
        {
            public readonly ICommand Command;
            public readonly TargetState State;

            public Entry(ICommand command, TargetState state)
            {
                Command = command;
                State = state;
            }
        }

        private Task[]? _consumerTasks;
        private readonly object _consumerTasksLock = new();

        public int TotalPendingCommands
        {
            get
            {
                var total = 0;
                for (var i = 0; i < _pendingCounts.Length; i++)
                    total += Volatile.Read(ref _pendingCounts[i]);
                return total;
            }
        }

        public MultiChannelCommandScheduler(int channelCount = 16)
        {
            if (channelCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(channelCount));

            _channels = new Channel<Entry>[channelCount];
            _pendingCounts = new int[channelCount];

            for (var i = 0; i < channelCount; i++)
            {
                _channels[i] = Channel.CreateUnbounded<Entry>(new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = true,
                });
            }
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

        public void ScheduleCommand(ICommand command)
        {
            var targetState = _targetStates.GetOrAdd(command.Target, static _ => new TargetState());
            targetState.Enqueue(new Entry(command, targetState), this);
        }

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

        public int PendingCommandCount(int channelIndex) => Volatile.Read(ref _pendingCounts[channelIndex]);

        public async Task DrainChannelAsync(int channelIndex)
        {
            var reader = _channels[channelIndex].Reader;

            while (Volatile.Read(ref _pendingCounts[channelIndex]) > 0)
            {
                while (reader.TryRead(out var entry))
                    ExecuteAndTrack(entry, channelIndex);

                if (Volatile.Read(ref _pendingCounts[channelIndex]) > 0)
                    await Task.Yield();
            }
        }
        
        private async Task ConsumeChannel(int channelIndex)
        {
            try
            {
                var reader = _channels[channelIndex].Reader;

                while (await reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
                {
                    while (reader.TryRead(out var entry))
                        ExecuteAndTrack(entry, channelIndex);
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

        private void ExecuteAndTrack(Entry entry, int channelIndex)
        {
            try
            {
                entry.Command.Execute();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Interlocked.Decrement(ref _pendingCounts[channelIndex]);
                entry.State.DecrementPending();
            }
        }

        private int FindLeastPendingChannel()
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

        private sealed class TargetState
        {
            private readonly object _gate = new();
            private int _pending;
            private int _channelIndex;

            public void Enqueue(Entry entry, MultiChannelCommandScheduler scheduler)
            {
                lock (_gate)
                {
                    var channelIndex = _pending > 0
                        ? _channelIndex
                        : AssignChannel(scheduler);

                    _pending++;
                    Interlocked.Increment(ref scheduler._pendingCounts[channelIndex]);

                    if (scheduler._channels[channelIndex].Writer.TryWrite(entry))
                        return;

                    Interlocked.Decrement(ref scheduler._pendingCounts[channelIndex]);
                    _pending--;
                    throw new InvalidOperationException("Channel was completed.");
                }
            }

            public void DecrementPending()
            {
                Interlocked.Decrement(ref _pending);
            }

            private int AssignChannel(MultiChannelCommandScheduler scheduler)
            {
                var index = scheduler.FindLeastPendingChannel();
                _channelIndex = index;
                return index;
            }
        }
    }
}