using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Schedulers.Implementations
{
    public class MultiChannelCommandScheduler : ChannelCommandSchedulerBase<MultiChannelCommandScheduler.Entry>
    {
        private readonly ConcurrentDictionary<object, TargetState> _targetStates = new();

        public int RegisteredTargetStateCount => _targetStates.Count;

        public MultiChannelCommandScheduler(int channelCount = 16)
            : base(channelCount)
        {
        }

        public override ValueTask ScheduleCommand(ICommand command)
        {
            var target = command.Target;
            while (true)
            {
                var targetState = GetState(target);
                var entry = new Entry(command, targetState);
                if (targetState.TryEnqueue(entry, this, awaitable: true, out var task))
                    return task;

                DropState(target, targetState);
            }
        }

        public override void SendCommand(ICommand command)
        {
            var target = command.Target;
            while (true)
            {
                var targetState = GetState(target);
                var entry = new Entry(command, targetState);
                if (targetState.TryEnqueue(entry, this, awaitable: false, out _))
                    return;

                DropState(target, targetState);
            }
        }

        protected override void ExecuteCommand(Entry entry) => entry.Command.Execute();
        protected override void OnCommandExecuted(Entry entry) => entry.State.DecrementPending(this, entry);
        
        private TargetState GetState(object target)
        {
            return _targetStates.GetOrAdd(target, static _ => new TargetState());
        }
        
        private void DropState(object target, TargetState targetState)
        {
            if (_targetStates.TryGetValue(target, out var state) && state == targetState)
                _targetStates.TryRemove(target, out _);
        }

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
            private int _channelIndex = -1;
            private bool _retired;
            
            public bool TryEnqueue(Entry entry, MultiChannelCommandScheduler scheduler, bool awaitable, out ValueTask task)
            {
                lock (_gate)
                {
                    if (_retired)
                    {
                        task = default;
                        return false;
                    }

                    var channelIndex = GetChannel(scheduler);

                    Interlocked.Increment(ref _pending);

                    if (awaitable)
                    {
                        if (scheduler.TryScheduleToChannel(channelIndex, entry, out task))
                            return true;
                    }
                    else
                    {
                        task = default;
                        if (scheduler.TryScheduleToChannel(channelIndex, entry))
                            return true;
                    }

                    Interlocked.Decrement(ref _pending);
                    throw new InvalidOperationException("Channel was completed.");
                }
            }
            
            public void DecrementPending(MultiChannelCommandScheduler scheduler, Entry entry)
            {
                if (Interlocked.Decrement(ref _pending) != 0)
                    return;

                lock (_gate)
                {
                    if (_retired || Volatile.Read(ref _pending) > 0)
                        return;

                    _retired = true;
                }

                scheduler.DropState(entry.Command.Target, this);
            }

            private int GetChannel(MultiChannelCommandScheduler scheduler)
            {
                lock (_gate)
                {
                    if (_channelIndex == -1)
                        _channelIndex = scheduler.FindLeastPendingChannel();
                    return _channelIndex;
                }
            }
        }
    }
}