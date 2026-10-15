using SecondScratch.ThreadSafe.Commands;
using SecondScratch.ThreadSafe.LockableObject;

namespace SecondScratch.ThreadSafe.Tests.Mocks
{
    public partial class Player : AbstractLockableObject
    {
        public IncreaseHealthCommand CreateIncreaseHealthCommand(int value, ICommandDependency? dependency = null) =>
            new(this, value, dependency);

        public class IncreaseHealthCommand : AbstractCommand<Player>
        {
            private readonly int _value;

            public IncreaseHealthCommand(Player target, int value, ICommandDependency? dependency = null)
                : base(target, dependency)
            {
                _value = value;
            }

            protected override void InnerExecute() => TypedTarget.IncreaseHealth(_value);
        }
    }
}