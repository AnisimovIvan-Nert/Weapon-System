using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Tests
{
    public class CommandScheduler : IDisposable
    {
        private readonly Channel<ICommand>[] _channels;
        private readonly int[] _pendingCounts;
        private readonly CancellationTokenSource _cts = new();
        private readonly ConditionalWeakTable<object, TargetState> _targetStates = new();
        private Task[]? _consumerTasks;

        public int ChannelCount => _channels.Length;

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

        public CommandScheduler(int channelCount = 16)
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

        public void ScheduleCommand(ICommand command)
        {
            var target = command.Target;
            var targetState = _targetStates.GetValue(target, static _ => new TargetState());

            var channelIndex = targetState.GetChanelIndex(this);

            targetState.IncrementPending();
            Interlocked.Increment(ref _pendingCounts[channelIndex]);

            if (_channels[channelIndex].Writer.TryWrite(command))
                return;

            Interlocked.Decrement(ref _pendingCounts[channelIndex]);
            targetState.DecrementPending();

            throw new InvalidOperationException();
        }

        public void RunConsumers()
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

        public int PendingCommandCount(int channelIndex) => Volatile.Read(ref _pendingCounts[channelIndex]);

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

        internal async Task DrainChannelAsync(int channelIndex)
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

        public void Dispose()
        {
            _cts.Cancel();
            foreach (var channel in _channels)
                channel.Writer.TryComplete();
            if (_consumerTasks != null) 
                Task.WaitAll(_consumerTasks);
            _cts.Dispose();
        }

        private sealed class TargetState
        {
            private int _pending;
            private int _channelIndex;

            public int GetChanelIndex(CommandScheduler commandScheduler)
            {
                if (Volatile.Read(ref _pending) > 0)
                    return Volatile.Read(ref _channelIndex);

                var index = commandScheduler.FindLeastPendingChannel();
                Volatile.Write(ref _channelIndex, index);
                return index;
            }

            public void IncrementPending()
            {
                Interlocked.Increment(ref _pending);
            }

            public void DecrementPending()
            {
                Interlocked.Decrement(ref _pending);
            }
        }
    }
}