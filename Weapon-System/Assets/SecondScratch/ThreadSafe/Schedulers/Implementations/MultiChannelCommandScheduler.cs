using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SecondScratch.Shared;
using SecondScratch.ThreadSafe.Commands;
using SecondScratch.ThreadSafe.LockableObject;

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

        public readonly struct Entry : ICommand
        {
            internal readonly ICommand Command;
            internal readonly TargetState State;

            internal Entry(ICommand command, TargetState state)
            {
                Command = command;
                State = state;
            }

            public ILockableObject Target => Command.Target;
            
            public bool IsCompleted => Command.IsCompleted;
            public void Execute() => Command.Execute();
            public bool CanExecute() => Command.CanExecute();

            public ConditionalEvent? OnReadyForExecute => Command.OnReadyForExecute;

            public ConditionalEvent OnCompleted => Command.OnCompleted;
        }

        internal sealed class TargetState
        {
            private readonly object _gate = new();
            private int _pending;
            private int _channelIndex = -1;
            private bool _retired;
            
            public bool TryEnqueue(Entry entry, MultiChannelCommandScheduler scheduler, bool awaitable, out ValueTask task)
            {
                task = default;
                
                lock (_gate)
                {
                    if (_retired)
                        return false;

                    var channelIndex = GetChannel(scheduler);

                    Interlocked.Increment(ref _pending);

                    try
                    {
                        if (awaitable)
                            scheduler.ScheduleToChannel(channelIndex, entry, out task);
                        else
                            scheduler.ScheduleToChannel(channelIndex, entry);

                        return true;
                    }
                    catch (Exception)
                    {
                        Interlocked.Decrement(ref _pending);
                        throw;
                    }
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