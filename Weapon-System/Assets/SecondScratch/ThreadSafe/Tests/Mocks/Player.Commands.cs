using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Tests.Mocks
{
    public partial class Player
    {
        public IncreaseHealthCommand CreateIncreaseHealthCommand(int value) => new(this, value);

        public class IncreaseHealthCommand : ICommand
        {
            private readonly Player _target;
            private readonly int _value;

            public object Target => _target;

            public IncreaseHealthCommand(Player target, int value)
            {
                _target = target;
                _value = value;
            }

            public void Execute() => _target.IncreaseHealth(_value);
        }
    }
}