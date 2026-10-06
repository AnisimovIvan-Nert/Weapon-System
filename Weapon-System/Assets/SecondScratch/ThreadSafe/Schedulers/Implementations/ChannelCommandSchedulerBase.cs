using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Commands;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Schedulers.Implementations
{
    public abstract class ChannelCommandSchedulerBase<TCommand> : ICommandScheduler
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly Channel<AwaitableCommand>[] _channels;
        private readonly int[] _pendingCounts;

        private CancellationTokenSource? _consumerCts;
        private CancellationTokenSource? _consumerLinkedCts;
        private Task[]? _consumerTasks;
        private readonly object _consumerTasksLock = new();

        public int ChannelCount => _channels.Length;

        protected ChannelCommandSchedulerBase(int channelCount)
        {
            if (channelCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(channelCount));

            _channels = new Channel<AwaitableCommand>[channelCount];
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

        public abstract ValueTask ScheduleCommand(ICommand command);
        public abstract void SendCommand(ICommand command);

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
                _consumerCts = new CancellationTokenSource();
                _consumerLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, _consumerCts.Token);

                for (var i = 0; i < _channels.Length; i++)
                {
                    var channelIndex = i;
                    _consumerTasks[i] = Task.Run(() => ConsumeChannel(channelIndex, _consumerLinkedCts));
                }
            }
        }

        public void KillConsumers()
        {
            lock (_consumerTasksLock)
            {
                if (_consumerTasks == null || _consumerCts == null || _consumerLinkedCts == null)
                    throw new InvalidOperationException();
                
                _consumerCts.Cancel();

                foreach (var consumerTask in _consumerTasks)
                {
                    consumerTask.Wait();
                    consumerTask.Dispose();
                }

                _consumerTasks = null;
                
                _consumerCts.Dispose();
                _consumerCts = null;
                
                _consumerLinkedCts.Dispose();
                _consumerLinkedCts = null;
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

        protected virtual void OnCommandExecuted(TCommand command)
        {
        }

        protected bool TryScheduleToChannel(int channelIndex, TCommand command)
        {
            var awaitableCommand = new AwaitableCommand(command, null);
            return TryScheduleToChannel(channelIndex, awaitableCommand);
        }
        
        protected bool TryScheduleToChannel(int channelIndex, TCommand command, out ValueTask task)
        {
            var taskSource = new TaskCompletionSource<bool>();
            task = new ValueTask(taskSource.Task);

            var awaitableCommand = new AwaitableCommand(command, taskSource);
            if (TryScheduleToChannel(channelIndex, awaitableCommand))
                return true;

            task = default;
            return false;
        }
        
        protected bool TryScheduleToChannel(int channelIndex, AwaitableCommand command)
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

        private async ValueTask ConsumeChannel(int channelIndex, CancellationTokenSource cts)
        {
            try
            {
                var reader = _channels[channelIndex].Reader;
                
                while (await reader.WaitToReadAsync(cts.Token).ConfigureAwait(false))
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

        private void ExecuteAndTrack(AwaitableCommand item, int channelIndex)
        {
            var command = item.Command;
            Exception? error = null;
            try
            {
                ExecuteCommand(command);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                error = e;
            }
            finally
            {
                Interlocked.Decrement(ref _pendingCounts[channelIndex]);
                OnCommandExecuted(command);

                if (item.Tcs != null)
                {
                    if (error != null)
                        item.Tcs.TrySetException(error);
                    else
                        item.Tcs.TrySetResult(true);
                }
            }
        }

        private static Channel<AwaitableCommand> CreateChannel() => Channel.CreateUnbounded<AwaitableCommand>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = true,
            });

        protected readonly struct AwaitableCommand
        {
            public TCommand Command { get; }
            public TaskCompletionSource<bool>? Tcs { get; }

            public AwaitableCommand(TCommand command, TaskCompletionSource<bool>? tcs)
            {
                Command = command;
                Tcs = tcs;
            }
        }
    }
}