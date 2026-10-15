using System;
using System.Linq;
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
        private readonly DeferredChanel<AwaitableCommand>[] _channels;
        private readonly ChanelConsumer _chanelConsumer;

        public int ChannelCount => _channels.Length;

        protected ChannelCommandSchedulerBase(int channelCount)
        {
            if (channelCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(channelCount));

            _chanelConsumer = new ChanelConsumer(channelCount);

            _channels = new DeferredChanel<AwaitableCommand>[channelCount];
            for (var i = 0; i < channelCount; i++)
            {
                var chanel = Channel.CreateUnbounded<AwaitableCommand>(
                    new UnboundedChannelOptions
                    {
                        SingleReader = true,
                        SingleWriter = false,
                        AllowSynchronousContinuations = true,
                    });
                _channels[i] = new DeferredChanel<AwaitableCommand>(chanel, ExecuteCommand);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            foreach (var channel in _channels)
                channel.Dispose();

            await _chanelConsumer.DisposeAsync();

            _cts.Dispose();
        }

        public int GetTotalPendingCommands() => _channels.Select(chanel => chanel.PendingCount).Sum();
        public int PendingCommandCount(int channelIndex) => _channels[channelIndex].PendingCount;

        public void RunConsumers() => _chanelConsumer.RunConsumers(ConsumeChannel, _cts);
        public Task KillConsumersAsync() => _chanelConsumer.KillConsumersAsync();

        public abstract ValueTask ScheduleCommand(ICommand command);
        public abstract void SendCommand(ICommand command);

        protected virtual void OnCommandExecuted(TCommand command)
        {
        }

        protected void ScheduleToChannel(int chanelIndex, TCommand command) =>
            ScheduleToChannel(chanelIndex, command, null);

        protected void ScheduleToChannel(int chanelIndex, TCommand command,
            out ValueTask task)
        {
            var tcs = new TaskCompletionSource<bool>();
            task = new ValueTask(tcs.Task);
            ScheduleToChannel(chanelIndex, command, tcs);
        }

        private void ScheduleToChannel(int chanelIndex, TCommand command, TaskCompletionSource<bool>? tcs)
        {
            var chanel = _channels[chanelIndex];
            var awaitableCommand = new AwaitableCommand(command, tcs);

            if (command.CanExecute())
                chanel.WriteToChannel(awaitableCommand);
            else
                command.OnReadyForExecute.CallOrSubscribe(() => chanel.WriteToChannel(awaitableCommand));
            
            chanel.IncrementPending();
        }

        protected int FindLeastPendingChannel()
        {
            var resultChanel = 0;
            var leastPending = int.MaxValue;

            for (var i = 0; i < _channels.Length; i++)
            {
                var chanel = _channels[i];
                var pending = chanel.PendingCount;

                if (chanel.PendingCount >= leastPending)
                    continue;

                leastPending = pending;
                resultChanel = i;
                if (pending == 0)
                    break;
            }

            return resultChanel;
        }

        private async ValueTask ConsumeChannel(int channelIndex, CancellationTokenSource cts)
        {
            var chanel = _channels[channelIndex];
            await chanel.ConsumeChannel(cts);
        }

        private void ExecuteCommand(AwaitableCommand awaitableCommand)
        {
            var command = awaitableCommand.Command;
            var tcs = awaitableCommand.Tcs;

            try
            {
                if (!command.CanExecute())
                    throw new InvalidOperationException();

                command.Execute();
                OnCommandExecuted(command);
                tcs?.SetResult(true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                tcs?.TrySetException(e);
            }
        }

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