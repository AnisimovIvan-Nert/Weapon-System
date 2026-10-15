using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Schedulers.Implementations
{
    public class ChannelCommandScheduler : ChannelCommandSchedulerBase<ICommand>
    {
        public ChannelCommandScheduler()
            : base(1)
        {
        }

        public override ValueTask ScheduleCommand(ICommand command)
        {
            ScheduleToChannel(0, command, out var task);
            return task;
        }

        public override void SendCommand(ICommand command) => ScheduleToChannel(0, command);
    }
}