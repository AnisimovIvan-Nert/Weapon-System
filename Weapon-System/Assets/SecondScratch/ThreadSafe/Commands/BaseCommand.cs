using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Commands
{
    public abstract class BaseCommand<T> : ICommand
        where T : notnull
    {
        protected readonly T TypedTarget;

        private bool _isCompleted;
        private int _executed;
        private TaskCompletionSource<bool>? _tcs;

        public object Target => TypedTarget;
        public bool IsCompleted => _isCompleted || Exception != null;
        public Exception? Exception { get; private set; }
        
        public BaseCommand(T typedTarget)
        {
            TypedTarget = typedTarget;
        }
        
        public void Execute()
        {
            try
            {
                if (Interlocked.Exchange(ref _executed, 1) != 0)
                    throw new InvalidOperationException("Multiple execution");
                
                InnerExecute();

                _isCompleted = true;
                _tcs?.TrySetResult(true);
            }
            catch (Exception e)
            {
                _tcs?.TrySetException(e);
                Exception = e;
                throw;
            }
        }

        public ValueTask WaitExecution()
        {
            if (Volatile.Read(ref _executed) != 0)
                return new ValueTask(Task.CompletedTask);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            tcs = Interlocked.CompareExchange(ref _tcs, tcs, null) ?? tcs;

            if (Volatile.Read(ref _executed) != 0)
                tcs.TrySetResult(true);

            return new ValueTask(tcs.Task);
        }
        
        protected abstract void InnerExecute();
    }
}