using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Schedulers;
using SecondScratch.ThreadSafe.Schedulers.Implementations;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Extensions
{
    public static class SchedulerExtensions
    {
        public static async Task DrainConsumers(this ICommandScheduler scheduler)
        {
            try
            {
                scheduler.RunConsumers();
                await Task.Yield();
            }
            catch (ConsumersAlreadyRunningException)
            {
            }
            finally
            {
                await scheduler.KillConsumersAsync();
            }
        }
    }
}