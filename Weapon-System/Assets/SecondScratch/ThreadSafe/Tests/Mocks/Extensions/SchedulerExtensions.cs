using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Schedulers;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Extensions
{
    public static class SchedulerExtensions
    {
        public static async Task DrainConsumers(this ICommandScheduler scheduler)
        {
            try
            {
                scheduler.RunConsumers();
            }
            catch (SchedulerAlreadyRunConsumersException)
            {
            }
            finally
            {
                await scheduler.KillConsumersAsync();
            }
        }
    }
}