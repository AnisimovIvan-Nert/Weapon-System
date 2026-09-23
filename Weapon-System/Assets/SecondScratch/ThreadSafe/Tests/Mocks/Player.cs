using System;
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

        public readonly struct IncreaseHealthCommand : ICommand
        {
            private readonly Player _target;
            private readonly int _value;

            private readonly bool[] _executed;
            private readonly TaskCompletionSource<bool>?[] _tcs;

            private bool Executed => _executed[0];
            private TaskCompletionSource<bool>? Tcs => _tcs[0];

            public object Target => _target;

            public IncreaseHealthCommand(Player target, int value)
            {
                _target = target;
                _value = value;

                _executed = new bool[1];
                _tcs = new TaskCompletionSource<bool>?[1];
            }

            public void Execute()
            {
                lock (_executed)
                {
                    if (Executed)
                    {
                        Tcs?.SetException(new InvalidOperationException("Multiple execution"));
                        throw new InvalidOperationException("Multiple execution");
                    }
                
                    _target.IncreaseHealth(_value);
                
                    _executed[0] = true;
                    Tcs?.SetResult(true);
                }
            }

            public ValueTask WaitExecution()
            {
                if (Executed)
                    return new ValueTask(Task.CompletedTask);
                
                lock (_executed)
                {
                    if (Executed)
                        return new ValueTask(Task.CompletedTask);

                    _tcs[0] = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    return new ValueTask(Tcs!.Task);
                }
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