using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture
{
    /// <summary>
    /// Dispatches actions to a specific thread (identified by its
    /// <see cref="SynchronizationContext"/> or thread-id).
    /// Objects that live on dedicated threads register here so
    /// interactions can safely reach them.
    /// </summary>
    public sealed class ThreadDispatcher
    {
        private readonly ConcurrentDictionary<string, SynchronizationContext> _contexts = new();

        /// <summary>
        /// Registers a named thread by capturing its current
        /// <see cref="SynchronizationContext"/>.
        /// Call this from the target thread during initialisation.
        /// </summary>
        public void Register(string threadName)
        {
            var context = SynchronizationContext.Current
                ?? throw new InvalidOperationException(
                    $"No SynchronizationContext on thread '{threadName}'. " +
                    "Call Register from a thread that has one (e.g. Unity main thread).");

            _contexts[threadName] = context;
        }

        /// <summary>
        /// Posts an action to the named thread.
        /// Returns a task that completes when the action finishes.
        /// </summary>
        public Task<T> InvokeAsync<T>(string threadName, Func<T> func)
        {
            if (!_contexts.TryGetValue(threadName, out var context))
                throw new KeyNotFoundException($"Thread '{threadName}' is not registered.");

            var completionSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            context.Post(_ =>
            {
                try
                {
                    var result = func();
                    completionSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    completionSource.SetException(ex);
                }
            }, null);

            return completionSource.Task;
        }

        /// <summary>
        /// Posts an action to the named thread.
        /// Returns a task that completes when the action finishes.
        /// </summary>
        public Task InvokeAsync(string threadName, Action action)
        {
            return InvokeAsync<object>(threadName, () =>
            {
                action();
                return null;
            });
        }

        public bool IsRegistered(string threadName) => _contexts.ContainsKey(threadName);
    }
}
