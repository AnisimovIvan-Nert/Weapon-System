using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Tests.Mocks
{
    public partial class Player
    {
        public IncreaseHealthCommand CreateIncreaseHealthCommand(int value) => new(this, value);

        public class IncreaseHealthCommand : BaseCommand<Player>
        {
            private readonly int _value;

            public IncreaseHealthCommand(Player target, int value)
                 : base(target)
            {
                _value = value;
            }

            protected override void InnerExecute() => TypedTarget.IncreaseHealth(_value);
        }
    }
}