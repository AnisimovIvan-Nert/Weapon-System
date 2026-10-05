using SecondScratch.ThreadSafe.Schedulers.Implementations;

namespace SecondScratch.ThreadSafe.Schedulers
{
    public static class CommandScheduler
    {
        private static ICommandScheduler _commandScheduler = new ChannelCommandScheduler();

        public static void SetScheduler(ICommandScheduler scheduler)
        {
            _commandScheduler = scheduler;
        }

        public static ICommandScheduler GetScheduler()
        {
            return _commandScheduler;
        }
    }
}