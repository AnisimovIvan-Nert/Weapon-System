using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Operations.TaskBasedImplementation
{
    public sealed class InteractionScheduler
    {
        private readonly List<IInteraction> _active = new();
        private readonly ConcurrentBag<IInteraction> _pendingAdd = new();

        // ReSharper disable once InconsistentlySynchronizedField
        public int ActiveCount => _active.Count;
        public int PendingCount => _pendingAdd.Count;
        public int TotalCount => ActiveCount + PendingCount;

        public Interaction<TContext> Schedule<TContext>(
            IEnumerable<InteractionStage<TContext>> stages,
            CancellationToken ct = default)
            where TContext : class, new()
        {
            var interaction = new Interaction<TContext>(stages, ct);
            _pendingAdd.Add(interaction);
            return interaction;
        }

        public async ValueTask Update()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            for (var i = 0; i < _active.Count; i++)
            {
                // ReSharper disable once InconsistentlySynchronizedField
                var interaction = _active[i];

                if (interaction.IsCompleted)
                {
                    lock (_active)
                        _active.RemoveAt(i);
                    i--;
                    continue;
                }

                try
                {
                    await interaction.ExecuteAsync().ConfigureAwait(false);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    Debug.LogError($"Interaction {interaction.Id} failed: {e}");
                }

                if (interaction.IsCompleted)
                {
                    lock (_active)
                        _active.RemoveAt(i);
                    i--;
                }
            }

            lock (_active)
            {
                while (_pendingAdd.TryTake(out var next))
                {
                    _active.Add(next);
                }
            }
        }

        public void CancelAll()
        {
            lock (_active)
            {
                foreach (var interaction in _active)
                    interaction.Cancel();
                foreach (var interaction in _pendingAdd)
                    interaction.Cancel();
            }
        }
    }
}