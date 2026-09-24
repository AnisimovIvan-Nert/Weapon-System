using System;
using System.Collections.Concurrent;
using System.Threading;
using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Schedulers
{
    public class MultiChannelCommandScheduler : ChannelCommandSchedulerBase<MultiChannelCommandScheduler.Entry>
    {
        private readonly ConcurrentDictionary<object, TargetState> _targetStates = new();

        public MultiChannelCommandScheduler(int channelCount = 16)
            : base(channelCount)
        {
        }

        public void ScheduleCommand(ICommand command)
        {
            var targetState = _targetStates.GetOrAdd(command.Target, static _ => new TargetState());
            targetState.Enqueue(new Entry(command, targetState), this);
        }

        protected override void ExecuteCommand(Entry entry) => entry.Command.Execute();
        protected override void OnCommandExecuted(Entry entry) => entry.State.DecrementPending();

        public readonly struct Entry
        {
            internal readonly ICommand Command;
            internal readonly TargetState State;

            internal Entry(ICommand command, TargetState state)
            {
                Command = command;
                State = state;
            }
        }

        internal sealed class TargetState
        {
            private readonly object _gate = new();
            private int _pending;
            private int _channelIndex;

            public void Enqueue(Entry entry, MultiChannelCommandScheduler scheduler)
            {
                lock (_gate)
                {
                    var channelIndex = _pending > 0
                        ? _channelIndex
                        : AssignChannel(scheduler);

                    _pending++;

                    if (scheduler.TryScheduleToChannel(channelIndex, entry))
                        return;

                    _pending--;
                    throw new InvalidOperationException("Channel was completed.");
                }
            }

            public void DecrementPending()
            {
                Interlocked.Decrement(ref _pending);
            }

            private int AssignChannel(MultiChannelCommandScheduler scheduler)
            {
                var index = scheduler.FindLeastPendingChannel();
                _channelIndex = index;
                return index;
            }
        }
    }
}