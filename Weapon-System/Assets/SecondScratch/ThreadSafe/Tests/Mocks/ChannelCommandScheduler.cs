using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Tests.Mocks
{
    public class ChannelCommandScheduler : IAsyncDisposable
    {
        private readonly Channel<ICommand> _channel = Channel.CreateUnbounded<ICommand>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = true,
        });

        private readonly CancellationTokenSource _cts = new();

        private int _pending;
        private Task? _consumerTask;
        private readonly object _consumerTaskLock = new();

        public int TotalPendingCommands => _pending;

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _channel.Writer.TryComplete();

            if (_consumerTask != null)
                await _consumerTask;

            _cts.Dispose();
        }

        public void ScheduleCommand(ICommand command)
        {
            Interlocked.Increment(ref _pending);
            
            if (_channel.Writer.TryWrite(command))
                return;
            
            Interlocked.Decrement(ref _pending);
            throw new InvalidOperationException("Channel was completed.");
        }

        public void RunConsumers()
        {
            lock (_consumerTaskLock)
            {
                if (_consumerTask != null)
                    throw new InvalidOperationException();

                _consumerTask = Task.Run(ConsumeChannel);
            }
        }

        public async Task DrainChannelAsync()
        {
            var reader = _channel.Reader;

            while (Volatile.Read(ref _pending) > 0)
            {
                while (reader.TryRead(out var command))
                    ExecuteAndTrack(command);

                if (Volatile.Read(ref _pending) > 0)
                    await Task.Yield();
            }
        }

        private async Task ConsumeChannel()
        {
            try
            {
                var reader = _channel.Reader;

                while (await reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
                {
                    while (reader.TryRead(out var command))
                        ExecuteAndTrack(command);
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

        private void ExecuteAndTrack(ICommand command)
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
                Interlocked.Decrement(ref _pending);
            }
        }
    }
}