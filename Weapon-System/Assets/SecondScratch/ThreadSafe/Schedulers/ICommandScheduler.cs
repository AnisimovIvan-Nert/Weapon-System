using System;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Schedulers
{
    public class SchedulerAlreadyRunConsumersException : Exception
    {
    }
    
    public interface ICommandScheduler : IAsyncDisposable
    {
        int ChannelCount { get; }
        
        ValueTask ScheduleCommand(ICommand command);
        void SendCommand(ICommand command);
        
        int GetTotalPendingCommands();
        int PendingCommandCount(int channelIndex);
        
        void RunConsumers();
        Task KillConsumersAsync();
    }
}