using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture
{
    /// <summary>
    /// Top-level facade: owns the scheduler, the cross-thread dispatcher,
    /// and tracks all running interactions.
    /// </summary>
    public sealed class InteractionWorld : IDisposable
    {
        private readonly HashSet<IInteraction> _tracked = new();

        public InteractionScheduler Scheduler { get; }
        public ThreadDispatcher Dispatcher { get; }
        
        public int TrackedCount
        {
            get { lock (_tracked) return _tracked.Count; }
        }

        public InteractionWorld(Func<Task> yield = null)
        {
            Dispatcher = new ThreadDispatcher();
            Scheduler = new InteractionScheduler(yield);
        }
        
        public void Dispose()
        {
            CancelAll();
        }
        
        /// <summary>Call every frame from a MonoBehaviour.</summary>
        public void Tick() => Scheduler.Tick().GetAwaiter().GetResult();

        /// <summary>Registers a named thread's SynchronizationContext.</summary>
        public void RegisterThread(string threadName) => Dispatcher.Register(threadName);

        /// <summary>Schedule and track an interaction.</summary>
        public Interaction<TContext> Schedule<TContext>(
            IEnumerable<InteractionStage<TContext>> stages,
            CancellationToken cancellationToken = default) where TContext : class, new()
        {
            var interaction = Scheduler.Schedule(stages, cancellationToken);
            lock (_tracked) 
                _tracked.Add(interaction);
            return interaction;
        }

        /// <summary>Remove an interaction from tracking.</summary>
        public void Remove<TContext>(Interaction<TContext> interaction)
            where TContext : class, new()
        {
            lock (_tracked)
                _tracked.Remove(interaction);
        }

        /// <summary>Cancels and rolling back all tracked interaction.</summary>
        public void CancelAll()
        {
            lock (_tracked)
                foreach (IInteraction interaction in _tracked)
                    interaction.Cancel();
        }
    }
}
