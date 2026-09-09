using System;
using System.Threading;

namespace Scratch.InteractionArchitecture.Containers
{
    /// <summary>
    /// THREAD AFFINITY: every object is owned by exactly one thread — either
    /// the main thread (Owner == null) or a dedicated
    /// <see cref="ThreadDispatcher.WorkerThread"/>. All access to the container
    /// must happen on its owner thread; this is what makes it safe without
    /// locks, because each container is only ever touched by a single thread.
    ///
    /// Use <see cref="RunOnOwner"/> to marshal an operation onto the owner
    /// thread from any other thread.
    /// </summary>
    public abstract class ThreadObject : IIdentifiable
    {
        public int Id { get; }
        public ThreadDispatcher.WorkerThread Owner { get; }

        /// <param name="owner">
        /// The thread that owns this container, or null to own it on the main
        /// thread (the caller's thread at construction time).
        /// </param>
        protected ThreadObject(int id, ThreadDispatcher.WorkerThread owner)
        {
            Id = id;
            Owner = owner;
        }

        /// <summary>Marshals an operation onto this container's owner thread.
        /// If the caller is already on the owner thread it runs inline.</summary>
        public T RunOnOwner<T>(Func<T> func)
        {
            if (Owner == null)
            {
                AssertMainThread();
                return func();
            }

            return Owner.InvokeSync(func);
        }

        public void RunOnOwner(Action action) => RunOnOwner<object>(() =>
        {
            action();
            return null;
        });

        protected void AssertOnOwner()
        {
            if (Owner != null && Thread.CurrentThread.ManagedThreadId != Owner.ThreadId)
                throw new InvalidOperationException(
                    $"{GetType().Name}:{Id} must be accessed on its owner thread " +
                    $"#{Owner.ThreadId}, but was touched on #{Thread.CurrentThread.ManagedThreadId}. " +
                    "Use RunOnOwner to marshal access.");
        }

        private void AssertMainThread()
        {
            if (Owner != null)
                throw new InvalidOperationException($"{GetType().Name}:{Id} has a worker owner.");
        }

        public override string ToString() => Owner != null ? $"@{Owner.Name}" : "@Main";
    }
}