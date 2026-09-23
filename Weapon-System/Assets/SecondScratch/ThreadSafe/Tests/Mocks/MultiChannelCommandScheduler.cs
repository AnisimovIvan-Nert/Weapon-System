using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Tests.Mocks
{
    public class MultiChannelCommandScheduler : IAsyncDisposable
    {
        private readonly Channel<ICommand>[] _channels;
        private readonly int[] _pendingCounts;
        private readonly CancellationTokenSource _cts = new();
        private readonly ConditionalWeakTable<object, TargetState> _targetStates = new();

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

            _channels = new Channel<ICommand>[channelCount];
            _pendingCounts = new int[channelCount];

            for (var i = 0; i < channelCount; i++)
            {
                _channels[i] = Channel.CreateUnbounded<ICommand>(new UnboundedChannelOptions
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
            var targetState = _targetStates.GetValue(command.Target, static _ => new TargetState());
            targetState.Enqueue(command, this);
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
                while (reader.TryRead(out var command))
                    ExecuteAndTrack(command, channelIndex);

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
                    while (reader.TryRead(out var command))
                        ExecuteAndTrack(command, channelIndex);
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

        private void ExecuteAndTrack(ICommand command, int channelIndex)
        {
            try
            {
                command.Execute();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Interlocked.Decrement(ref _pendingCounts[channelIndex]);
                if (_targetStates.TryGetValue(command.Target, out var state))
                    state.DecrementPending();
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

            public void Enqueue(ICommand command, MultiChannelCommandScheduler scheduler)
            {
                lock (_gate)
                {
                    var channelIndex = _pending > 0
                        ? _channelIndex
                        : AssignChannel(scheduler);

                    _pending++;
                    Interlocked.Increment(ref scheduler._pendingCounts[channelIndex]);

                    if (scheduler._channels[channelIndex].Writer.TryWrite(command))
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