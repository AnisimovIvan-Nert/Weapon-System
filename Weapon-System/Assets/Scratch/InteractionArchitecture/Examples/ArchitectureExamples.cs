using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Scratch.InteractionArchitecture.Examples
{
    // ------------------------------------------------------------------- //
    // Shared context for the example interaction.
    // ------------------------------------------------------------------- //
    public sealed class CounterContext
    {
        public Counter Counter { get; set; } = new();
    }

    public sealed class Counter
    {
        public int Value;
    }

    // ------------------------------------------------------------------- //
    // A stage that spreads work across many frames by yielding each step.
    // ------------------------------------------------------------------- //
    public sealed class IncrementStage : InteractionStage<CounterContext>
    {
        private readonly int _steps;

        public IncrementStage(int steps) => _steps = steps;

        public override string Name => $"Increment x{_steps}";

        public override async Task ExecuteAsync(
            Transaction transaction,
            CounterContext context,
            Func<Task> yield,
            CancellationToken cancellationToken)
        {
            for (var i = 0; i < _steps; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Every mutation is recorded via a compensation so that if
                // the interaction is cancelled it is fully undone.
                context.Counter.Value = transaction.Apply(
                    mutation: () => context.Counter.Value + 1,
                    compensationFactory: oldValue => () => context.Counter.Value = oldValue);

                //TODO how works yield
                await yield(); // suspend until next frame
            }
        }
    }

    // ------------------------------------------------------------------- //
    // A stage that deliberately throws to demonstrate automatic rollback.
    // ------------------------------------------------------------------- //
    public sealed class FailStage : InteractionStage<CounterContext>
    {
        public override string Name => "Fail";

        public override Task ExecuteAsync(
            Transaction transaction,
            CounterContext context,
            Func<Task> yield,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Deliberate failure.");
        }
    }

    // ------------------------------------------------------------------- //
    // A stage that talks to an object living on a dedicated worker thread.
    // Access is marshalled through the InteractionWorld dispatcher.
    // ------------------------------------------------------------------- //
    public sealed class BackgroundObjectStage : InteractionStage<CounterContext>
    {
        private readonly InteractionWorld _world;
        private readonly string _threadName;

        public BackgroundObjectStage(InteractionWorld world, string threadName)
        {
            _world = world;
            _threadName = threadName;
        }

        public override string Name => $"Increment background object on '{_threadName}'";

        public override async Task ExecuteAsync(
            Transaction transaction,
            CounterContext context,
            Func<Task> yield,
            CancellationToken cancellationToken)
        {
            var result = await _world.Dispatcher.InvokeAsync(_threadName, () =>
            {
                // This lambda executes on the owner thread of _threadName.
                return context.Counter.Value + 1;
            });

            transaction.RegisterCompensation(() =>
            {
                _world.Dispatcher.InvokeAsync<object>(_threadName, () =>
                {
                    // Undo on the owner thread.
                    return null;
                });
            });
        }
    }

    // ------------------------------------------------------------------- //
    // MonoBehaviour that owns the world and pumps it every frame.
    // ------------------------------------------------------------------- //
    public class InteractionRunner : MonoBehaviour
    {
        public InteractionWorld World { get; private set; }
        
        private void Awake()
        {
            // Unity main thread context is registered under "Main".
            SynchronizationContext.SetSynchronizationContext(new UnitySyncContext());
            World = new InteractionWorld();
            World.RegisterThread("Main");

            StartCoroutine(TickRoutine());
        }

        private IEnumerator TickRoutine()
        {
            while (World != null)
            {
                Exception error = null;
                yield return RunAndCapture(World.Tick(), ex => error = ex);

                if (error != null)
                    Debug.LogError($"[TickRoutine] {error}");

                yield return null;
            }
        }

        private static IEnumerator RunAndCapture(Task task, Action<Exception> onError)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
                onError(task.Exception);
            else if (task.IsCanceled)
                onError(new OperationCanceledException("Tick was cancelled."));
        }

        private void OnDestroy() => World?.Dispose();
    }

    /// <summary>Captures the Unity main-thread sync context so the dispatcher works.</summary>
    public sealed class UnitySyncContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object state)
        {
            UnityMainThreadQueue.Enqueue(() => d(state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            UnityMainThreadQueue.EnqueueAndWait(() => d(state));
        }
    }

    /// <summary>Simple queue that runs posted callbacks on the Unity main thread at Update.</summary>
    public static class UnityMainThreadQueue
    {
        private static readonly ConcurrentQueue<Action> Queue = new();
        private static readonly object Gate = new();

        public static void Enqueue(Action action) => Queue.Enqueue(action);

        public static void EnqueueAndWait(Action action)
        {
            Exception error = null;
            var done = false;
            Queue.Enqueue(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    lock (Gate)
                    {
                        done = true; 
                        Monitor.Pulse(Gate);
                    }
                }
            });
            lock (Gate)
            {
                while (!done) 
                    Monitor.Wait(Gate);
            }
            if (error != null) 
                throw error;
        }

        //TODO When it's runs
        public static void Drain()
        {
            while (Queue.TryDequeue(out var action))
                action();
        }
    }
}
