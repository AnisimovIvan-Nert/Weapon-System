using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture
{
    /// <summary>
    /// Dispatches actions to a specific thread. A thread is identified by its
    /// <see cref="SynchronizationContext"/>: either the context captured on the
    /// registering thread (e.g. Unity main thread via <see cref="Register"/>), or
    /// a <see cref="QueueSynchronizationContext"/> spawned by
    /// <see cref="CreateWorkerThread"/>, which owns its own message-loop thread.
    /// Either way the dispatch path is exactly the same —
    /// <see cref="SynchronizationContext.Post"/>.
    /// </summary>
    public sealed class ThreadDispatcher
    {
        private readonly ConcurrentDictionary<string, SynchronizationContext> _contexts = new();

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
        /// Spawns a dedicated message-loop thread and installs a
        /// <see cref="QueueSynchronizationContext"/> on it. Use this for objects
        /// that "live on their own thread": every owned object is marshalled to
        /// it via plain <see cref="SynchronizationContext.Post"/>, exactly like
        /// the main thread. Call <see cref="QueueSynchronizationContext.Dispose"/>
        /// to stop the thread.
        /// </summary>
        public QueueSynchronizationContext CreateWorkerThread(string threadName)
        {
            var worker = new QueueSynchronizationContext(threadName);
            _contexts[threadName] = worker;
            return worker;
        }

        /// <summary>Gets a previously registered thread's context, or null.</summary>
        public SynchronizationContext? GetRegisteredThread(string threadName) =>
            _contexts.TryGetValue(threadName, out var context) ? context : null;

        /// <summary>Posts an action to the named thread and waits for completion.</summary>
        public Task<T> InvokeAsync<T>(string threadName, Func<T> func)
        {
            if (_contexts.TryGetValue(threadName, out var context))
                return InvokeOnContext(context, func);

            throw new KeyNotFoundException($"Thread '{threadName}' is not registered.");
        }

        public Task InvokeAsync(string threadName, Action action) =>
            InvokeAsync<object?>(threadName, () => { action(); return null; });

        public bool IsRegistered(string threadName) =>
            _contexts.ContainsKey(threadName);

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
        /// A <see cref="SynchronizationContext"/> that owns a dedicated message
        /// loop on a single background thread. Posting runs the callback on that
        /// thread; awaiting inside a callback resumes there too, because the
        /// context is installed on the thread. This makes the worker behave
        /// exactly like the main thread's Unity context, so every owned object
        /// uses one dispatch path.
        /// </summary>
        public sealed class QueueSynchronizationContext : SynchronizationContext, IDisposable
        {
            private readonly BlockingCollection<Action> _queue = new();
            private readonly string _name;
            private readonly int _threadId;

            public string Name => _name;

            /// <summary>The managed thread id of the owned message-loop thread.</summary>
            public int ThreadId => _threadId;

            public QueueSynchronizationContext(string name)
            {
                _name = name;
                var thread = new Thread(Run) { Name = name, IsBackground = true };
                thread.Start();
                _threadId = thread.ManagedThreadId;
            }

            /// <summary>Queues the callback onto the bound thread. Async-safe.</summary>
            public override void Post(SendOrPostCallback d, object? state)
            {
                _queue.Add(() => d(state));
            }

            /// <summary>Blocks the caller until the callback has run on the bound thread.</summary>
            public override void Send(SendOrPostCallback d, object? state)
            {
                if (Thread.CurrentThread.ManagedThreadId == _threadId)
                {
                    d(state);
                    return;
                }

                var completionSource = new TaskCompletionSource<object?>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                _queue.Add(() =>
                {
                    try
                    {
                        d(state);
                        completionSource.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        completionSource.SetException(ex);
                    }
                });

                completionSource.Task.GetAwaiter().GetResult();
            }

            private void Run()
            {
                SetSynchronizationContext(this);
                while (true)
                {
                    // TryTake with an infinite timeout returns false (instead of
                    // throwing like Take()) once the queue is empty and adding
                    // has been completed — i.e. the disarm point of Dispose().
                    if (!_queue.TryTake(out Action? action, Timeout.Infinite) || action == null)
                        break;

                    action();
                }
            }

            /// <summary>Stops the message loop once the queue drains. Safe to call from any thread.</summary>
            public void Dispose() => _queue.CompleteAdding();

            public override string ToString() => $"@{_name}";
        }
    }
}