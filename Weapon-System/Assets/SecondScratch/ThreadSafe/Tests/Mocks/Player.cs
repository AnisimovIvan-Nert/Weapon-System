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

            private readonly TaskCompletionSource<bool> _tcs;
            public Task ExecutionTask => _tcs.Task;
            
            public object Target => _target;

            public IncreaseHealthCommand(Player target, int value)
            {
                _target = target;
                _value = value;
                
                _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public void Execute()
            {
                if (ExecutionTask.IsCompleted)
                    throw new InvalidOperationException("Multiple calls");
                
                _target.IncreaseHealth(_value);
                
                _tcs.TrySetResult(true);
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