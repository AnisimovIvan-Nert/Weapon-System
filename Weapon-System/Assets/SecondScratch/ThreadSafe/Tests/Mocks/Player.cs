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

        public class IncreaseHealthCommand : ICommand
        {
            private readonly Player _target;
            private readonly int _value;

            private bool _executed;
            private TaskCompletionSource<bool>? _tcs;

            public object Target => _target;

            public IncreaseHealthCommand(Player target, int value)
            {
                _target = target;
                _value = value;
            }

            public void Execute()
            {
                lock (this)
                {
                    if (_executed)
                    {
                        _tcs?.SetException(new InvalidOperationException("Multiple execution"));
                        throw new InvalidOperationException("Multiple execution");
                    }
                
                    _target.IncreaseHealth(_value);
                
                    _executed = true;
                    _tcs?.SetResult(true);
                }
            }

            public ValueTask WaitExecution()
            {
                if (_executed)
                    return new ValueTask(Task.CompletedTask);
                
                lock (this)
                {
                    if (_executed)
                        return new ValueTask(Task.CompletedTask);

                    _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    return new ValueTask(_tcs.Task);
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