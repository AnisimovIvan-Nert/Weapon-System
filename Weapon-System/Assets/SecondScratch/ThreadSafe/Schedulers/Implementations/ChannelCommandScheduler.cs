using System;
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
            if (TryScheduleToChannel(0, command, out var task))
                return task;

            throw new InvalidOperationException("Channel was completed.");
        }

        public override void SendCommand(ICommand command)
        {
            if (TryScheduleToChannel(0, command))
                return;

            throw new InvalidOperationException("Channel was completed.");
        }

        protected override void ExecuteCommand(ICommand command) => command.Execute();
    }
}