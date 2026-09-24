using System;
using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Schedulers
{
    public class ChannelCommandScheduler : ChannelCommandSchedulerBase<ICommand>
    {
        public ChannelCommandScheduler()
            : base(1)
        {
        }

        public void ScheduleCommand(ICommand command)
        {
            if (TryScheduleToChannel(0, command))
                return;

            throw new InvalidOperationException("Channel was completed.");
        }

        protected override void ExecuteCommand(ICommand command) => command.Execute();
    }
}