using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Schedulers
{
    public static class CommandExtensions
    {
        public static ValueTask Schedule<T>(this T command)
            where T : ICommand
        {
            return CommandScheduler.GetScheduler().ScheduleCommand(command);
        }
        
        public static void Send<T>(this T command)
            where T : ICommand
        {
            CommandScheduler.GetScheduler().SendCommand(command);
        }
    }
}