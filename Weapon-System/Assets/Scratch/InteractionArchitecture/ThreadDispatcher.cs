using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture
{
    /// <summary>
    /// Dispatches actions to a specific thread.  A thread can be registered
    /// either by its current <see cref="SynchronizationContext"/> (e.g. Unity
    /// main thread) or, more commonly for worker threads that have no context,
    /// by spawning a dedicated message-loop thread via
    /// <see cref="CreateWorkerThread"/>.
    /// </summary>
    public sealed class ThreadDispatcher
    {
        private readonly ConcurrentDictionary<string, SynchronizationContext> _contexts = new();
        private readonly ConcurrentDictionary<string, WorkerThread> _workers = new();

        /// <summary>
        /// Registers a named thread by capturing its current
        /// <see cref="SynchronizationContext"/>. Call this from the target thread.
        /// </summary>
        public SynchronizationContext Register(string threadName)
        {
            var context = SynchronizationContext.Current
                ?? throw new InvalidOperationException(
                    $"No SynchronizationContext on thread '{threadName}'. " +
                    "Call Register from a thread that has one (e.g. Unity main thread).");

            _contexts[threadName] = context;
            return context;
        }

        /// <summary>
        /// Spawns and registers a dedicated worker thread with a message pump.
        /// Use this for objects that "live on their own thread" — the returned
        /// thread-owner has no <see cref="SynchronizationContext"/> but still
        /// accepts marshalled invocations.  Call <see cref="WorkerThread.Dispose"/>
        /// to stop it.
        /// </summary>
        public WorkerThread CreateWorkerThread(string threadName)
        {
            var worker = new WorkerThread(threadName);
            _workers[threadName] = worker;
            return worker;
        }

        public WorkerThread GetWorkerThread(string threadName) =>
            _workers.TryGetValue(threadName, out var worker) ? worker : null;

        /// <summary>Posts an action to the named thread and waits for completion.</summary>
        public Task<T> InvokeAsync<T>(string threadName, Func<T> func)
        {
            if (_contexts.TryGetValue(threadName, out var context))
                return InvokeOnContext(context, func);

            if (_workers.TryGetValue(threadName, out var worker))
                return worker.PostAsync(func);

            throw new KeyNotFoundException($"Thread '{threadName}' is not registered.");
        }

        public Task InvokeAsync(string threadName, Action action) =>
            InvokeAsync<object>(threadName, () => { action(); return null; });

        public bool IsRegistered(string threadName) =>
            _contexts.ContainsKey(threadName) || _workers.ContainsKey(threadName);

        private static Task<T> InvokeOnContext<T>(SynchronizationContext context, Func<T> func)
        {
            var completionSource = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            context.Post(_ =>
            {
                try
                {
                    completionSource.SetResult(func());
                }
                catch (Exception ex)
                {
                    completionSource.SetException(ex);
                }
            }, null);

            return completionSource.Task;
        }

        /// <summary>
        /// A dedicated thread running a message loop.  Work posted to it is
        /// executed on exactly that thread, giving each owned object a single
        /// serial access point without locks.
        /// </summary>
        public sealed class WorkerThread : IDisposable
        {
            private readonly BlockingCollection<Action> _queue = new();
            private readonly Thread _thread;
            private volatile bool _running = true;

            public string Name { get; }
            public int ThreadId { get; }

            public WorkerThread(string name)
            {
                Name = name;
                _thread = new Thread(Loop) { Name = name, IsBackground = true };
                _thread.Start();
                ThreadId = _thread.ManagedThreadId;
            }

            /// <summary>Posts <paramref name="func"/> to the worker thread and
            /// returns a task that completes when it has run there.</summary>
            public Task<T> PostAsync<T>(Func<T> func)
            {
                var completionSource = new TaskCompletionSource<T>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                _queue.Add(() =>
                {
                    try
                    {
                        completionSource.SetResult(func());
                    }
                    catch (Exception ex)
                    {
                        completionSource.SetException(ex);
                    }
                });

                return completionSource.Task;
            }

            public Task PostAsync(Action action) =>
                PostAsync<object>(() => { action(); return null; });

            /// <summary>Synchronously blocks the *calling* thread until the work
            /// has executed on this worker thread. Safe because the worker thread
            /// is a separate OS thread, not the caller.</summary>
            public T InvokeSync<T>(Func<T> func)
            {
                if (Thread.CurrentThread.ManagedThreadId == ThreadId)
                    return func();

                return PostAsync(func).GetAwaiter().GetResult();
            }

            private void Loop()
            {
                try
                {
                    while (_running)
                    {
                        Action action;
                        try
                        {
                            action = _queue.Take();
                        }
                        catch (ObjectDisposedException)
                        {
                            break;
                        }
                        action();
                    }
                }
                finally
                {
                    // Drain; skip remaining (graceful shutdown).
                }
            }

            public void Dispose()
            {
                _running = false;
                _queue.CompleteAdding();
            }
        }
    }
}
