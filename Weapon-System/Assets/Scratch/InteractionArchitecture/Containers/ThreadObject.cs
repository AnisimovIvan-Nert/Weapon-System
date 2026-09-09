using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture.Containers
{
    /// <summary>
    /// THREAD AFFINITY: every object is owned by exactly one thread — either a
    /// dedicated <see cref="ThreadDispatcher.WorkerThread"/> or, when created
    /// without an explicit owner, the thread that created it (e.g. the Unity
    /// main thread). All access must happen on its owner thread; this is what
    /// makes it safe without locks, because each container is only ever touched
    /// by a single thread.
    ///
    /// Use <see cref="RunOnOwner"/> to marshal an operation onto the owner
    /// thread from any other thread. For a worker-owned object that goes through
    /// the worker's message loop; for an ownerless object the operation is
    /// marshalled onto the creating thread via its
    /// <see cref="SynchronizationContext"/>.
    /// </summary>
    public abstract class ThreadObject : IIdentifiable
    {
        public int Id { get; }
        public ThreadDispatcher.WorkerThread? Owner { get; }

        private readonly int _ownerThreadId;
        private readonly SynchronizationContext? _ownerContext;

        /// <param name="owner">
        /// The worker thread that owns this object, or null to own it on the
        /// thread that creates it (the caller's thread at construction time).
        /// </param>
        protected ThreadObject(int id, ThreadDispatcher.WorkerThread? owner)
        {
            Id = id;
            Owner = owner;
            _ownerThreadId = owner?.ThreadId ?? Thread.CurrentThread.ManagedThreadId;
            _ownerContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Marshals an operation onto this object's owner thread. If the caller
        /// is already on the owner thread it runs inline.
        /// </summary>
        public T RunOnOwner<T>(Func<T> func)
        {
            if (Owner != null)
                return Owner.InvokeSync(func);

            if (Thread.CurrentThread.ManagedThreadId == _ownerThreadId)
                return func();
            
            if (_ownerContext == null)
                throw new InvalidOperationException(
                    $"{GetType().Name}:{Id} was created on a thread without a " +
                    "SynchronizationContext; it cannot be marshalled onto from another thread.");

            return DispatchOnContext(_ownerContext, func);
        }

        public void RunOnOwner(Action action) => RunOnOwner<object?>(() =>
        {
            action();
            return null;
        });

        private static T DispatchOnContext<T>(SynchronizationContext context, Func<T> func)
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

            return completionSource.Task.GetAwaiter().GetResult();
        }

        protected void AssertOnOwner()
        {
            if (Owner != null && Thread.CurrentThread.ManagedThreadId != Owner.ThreadId)
                throw new InvalidOperationException(
                    $"{GetType().Name}:{Id} must be accessed on its owner thread " +
                    $"#{Owner.ThreadId}, but was touched on #{Thread.CurrentThread.ManagedThreadId}. " +
                    "Use RunOnOwner to marshal access.");

            if (Owner == null && Thread.CurrentThread.ManagedThreadId != _ownerThreadId)
                throw new InvalidOperationException(
                    $"{GetType().Name}:{Id} is owned by the thread it was created on " +
                    $"#{_ownerThreadId}, but was touched on #{Thread.CurrentThread.ManagedThreadId}. " +
                    "Use RunOnOwner to marshal access.");
        }

        public override string ToString() => Owner != null ? $"@{Owner.Name}" : "@Main";
    }
}
