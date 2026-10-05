using System;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Schedulers
{
    public interface ICommandScheduler : IAsyncDisposable
    {
        int ChannelCount { get; }
        
        ValueTask ScheduleCommand(ICommand command);
        void SendCommand(ICommand command);
        
        int GetTotalPendingCommands();
        int PendingCommandCount(int channelIndex);
        
        void RunConsumers();
        void KillConsumers();
        
        Task DrainAsync();
    }
}