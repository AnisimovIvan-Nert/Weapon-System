using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture
{
    /// <summary>
    /// Frame-aware scheduler that pumps interactions forward each Unity tick.
    /// Interactions that <c>yield</c> suspend until the next tick, so heavy work
    /// is automatically distributed across frames.  Multiple interactions run
    /// concurrently and independently.
    /// </summary>
    public sealed class InteractionScheduler
    {
        private const int DefaultMaxPerFrame = 16;
        
        private readonly List<IInteraction> _active = new();
        private readonly List<IInteraction> _pending = new();
        private readonly object _lock = new();
        private readonly Func<Task> _yield;

        /// <summary>Maximum interactions to advance per tick.</summary>
        public int MaxPerFrame { get; set; } = DefaultMaxPerFrame;

        public int ActiveCount
        {
            get { lock (_lock) return _active.Count; }
        }

        public int PendingCount
        {
            get { lock (_lock) return _pending.Count; }
        }

        public InteractionScheduler(Func<Task>? yield = null)
        {
            _yield = yield ?? (() => Task.CompletedTask);
        }

        /// <summary>Builds and schedules a typed interaction.</summary>
        public Interaction<TContext> Schedule<TContext>(
            IEnumerable<InteractionStage<TContext>> stages,
            CancellationToken cancellationToken = default) 
            where TContext : class, new()
        {
            var interaction = new Interaction<TContext>(stages, cancellationToken);
            Enqueue(interaction);
            return interaction;
        }

        private void Enqueue(IInteraction interaction)
        {
            lock (_lock)
                _pending.Add(interaction);
        }

        /// <summary>
        /// Called once per frame.  Advances each active interaction by one step
        /// (a step runs synchronously until the stage yields), then promotes
        /// waiting interactions up to <see cref="MaxPerFrame"/>.
        /// </summary>
        public async Task Tick()
        {
            List<IInteraction> batch;

            lock (_lock)
                batch = new List<IInteraction>(_active);

            foreach (var interaction in batch)
            {
                if (interaction.IsCompleted)
                {
                    lock (_lock) 
                        _active.Remove(interaction);
                    continue;
                }

                try
                {
                    await interaction.AdvanceAsync(_yield).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    UnityEngine.Debug.LogError(
                        $"[InteractionScheduler] Interaction {interaction.Id} failed: {ex}");
                }

                if (interaction.IsCompleted)
                {
                    lock (_lock) 
                        _active.Remove(interaction);
                }
            }

            var spawned = 0;

            lock (_lock)
            {
                while (_pending.Count > 0 && spawned < MaxPerFrame)
                {
                    var next = _pending[0];
                    _pending.RemoveAt(0);
                    _active.Add(next);
                    spawned++;
                }
            }
        }

        public void CancelAll()
        {
            lock (_lock)
            {
                foreach (var interaction in _active)
                    interaction.Cancel();
                foreach (var interaction in _pending)
                    interaction.Cancel();
            }
        }
    }

    /// <summary>Non-generic view over a typed interaction for the scheduler.</summary>
    internal interface IInteraction
    {
        Guid Id { get; }
        bool IsCompleted { get; }
        Task AdvanceAsync(Func<Task> yield);
        void Cancel();
    }
}
