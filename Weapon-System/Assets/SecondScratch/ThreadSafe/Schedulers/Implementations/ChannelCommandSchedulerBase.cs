using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Commands;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Schedulers.Implementations
{
    public abstract class ChannelCommandSchedulerBase<TCommand> : ICommandScheduler
        where TCommand : ICommand
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

            _pendingCounts = new int[channelCount];

            _channels = new Channel<AwaitableCommand>[channelCount];
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
                    throw new SchedulerAlreadyRunConsumersException();

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
        
        public async Task KillConsumersAsync()
        {
            lock (_consumerTasksLock)
            {
                if (_consumerTasks == null || _consumerCts == null || _consumerLinkedCts == null)
                    return;

                _consumerCts.Cancel();
            }

            await Task.WhenAll(_consumerTasks);

            lock (_consumerTasksLock)
            {
                _consumerTasks = null;

                _consumerCts.Dispose();
                _consumerCts = null;

                _consumerLinkedCts.Dispose();
                _consumerLinkedCts = null;
            }
        }

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
                    await ReadCommands(channelIndex, reader);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async ValueTask ReadCommands(int channelIndex, ChannelReader<AwaitableCommand> reader)
        {
            var deferredCommands = _deferredCommands[channelIndex];
            while (true)
            {
                while (reader.TryRead(out var item))
                    TryExecuteCommand(item, channelIndex);

                for (var i = 0; i < deferredCommands.Count; i++)
                {
                    if (!deferredCommands.TryDequeue(out var item))
                        break;
                    
                    TryExecuteCommand(item, channelIndex);
                }

                if (deferredCommands.Count == 0)
                    break;

                await Task.Yield();
            }
        }

        private bool TryExecuteCommand(AwaitableCommand awaitableCommand, int channelIndex)
        {
            var command = awaitableCommand.Command;
            Exception? error = null;
            var executed = true;
            try
            {
                executed = TryExecuteCommand(command);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                error = e;
            }
            finally
            {
                if (executed)
                {
                    Interlocked.Decrement(ref _pendingCounts[channelIndex]);
                    OnCommandExecuted(command);

                    if (awaitableCommand.Tcs != null)
                    {
                        if (error != null)
                            awaitableCommand.Tcs.TrySetException(error);
                        else
                            awaitableCommand.Tcs.TrySetResult(true);
                    }
                }
            }

            return executed;
        }

        private static bool TryExecuteCommand(TCommand command)
        {
            if (!command.CanExecute())
                return false;

            if (!Monitor.TryEnter(command.Target))
                return false;

            try
            {
                command.Execute();
                return true;
            }
            finally
            {
                Monitor.Exit(command.Target);
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