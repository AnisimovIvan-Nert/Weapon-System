using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Schedulers.Implementations
{
    public delegate ValueTask ConsumeChanel(int chanelIndex, CancellationTokenSource cts);
    
    public class ConsumersAlreadyRunningException : Exception
    {
    }
    
    public class ChanelConsumer : IAsyncDisposable
    {
        private readonly int _chanelCount;
        
        private CancellationTokenSource? _cts;
        private CancellationTokenSource? _chanelLinkedCts;
        
        private Task[]? _consumerTasks;
        private readonly object _tasksLock = new();

        public ChanelConsumer(int chanelCount)
        {
            _chanelCount = chanelCount;
        }

        public async ValueTask DisposeAsync()
        {
            if (_consumerTasks != null)
                await Task.WhenAll(_consumerTasks);
            
            _cts?.Dispose();
            _chanelLinkedCts?.Dispose();
        }
        
        public void RunConsumers(ConsumeChanel consumeChanel, CancellationTokenSource chanelCts)
        {
            lock (_tasksLock)
            {
                if (_consumerTasks != null)
                    throw new ConsumersAlreadyRunningException();

                _consumerTasks = new Task[_chanelCount];
                
                _cts = new CancellationTokenSource();
                _chanelLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, chanelCts.Token);

                for (var i = 0; i < _chanelCount; i++)
                {
                    var chanelIndex = i;
                    _consumerTasks[i] = Task.Run(() => consumeChanel(chanelIndex, _chanelLinkedCts));
                }
            }
        }
        
        public async Task KillConsumersAsync()
        {
            lock (_tasksLock)
            {
                if (_consumerTasks == null || _cts == null || _chanelLinkedCts == null)
                    return;

                _cts.Cancel();
            }

            await Task.WhenAll(_consumerTasks);

            lock (_tasksLock)
            {
                _consumerTasks = null;

                _cts.Dispose();
                _cts = null;

                _chanelLinkedCts.Dispose();
                _chanelLinkedCts = null;
            }
        }
    }
}