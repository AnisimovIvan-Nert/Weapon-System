using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Tests.Mocks
{
    public partial class Player
    {
        public int Health { get; private set; }

        public void IncreaseHealth(int value)
        {
            Health += value;
        }

        public void Reset()
        {
            Health = 0;
        }
    }
    
    //commands
    public partial class Player
    {
        public IncreaseHealthCommand CreateIncreaseHealthCommand(int value) => new(this, value);

        public class IncreaseHealthCommand : ICommand
        {
            private readonly Player _target;
            private readonly int _value;

            private int _executed;
            private TaskCompletionSource<bool>? _tcs;

            public object Target => _target;

            public IncreaseHealthCommand(Player target, int value)
            {
                _target = target;
                _value = value;
            }

            public void Execute()
            {
                if (Interlocked.Exchange(ref _executed, 1) != 0)
                {
                    _tcs?.TrySetException(new InvalidOperationException("Multiple execution"));
                    throw new InvalidOperationException("Multiple execution");
                }

                _target.IncreaseHealth(_value);
                _tcs?.TrySetResult(true);
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
        }
    }

    //view
    public partial class Player
    {
        public View ToModel() => new(Health);

        public readonly struct View
        {
            public int Health { get; }

            public View(int health)
            {
                Health = health;
            }
        }
    }
}