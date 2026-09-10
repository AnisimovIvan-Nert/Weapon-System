using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture.Containers.Owner
{
    /// <summary>
    /// THREAD AFFINITY: every object is owned by exactly one thread. Ownership
    /// is identified by a <see cref="SynchronizationContext"/>: the main thread's
    /// context (or the context that was current at construction) for ownerless
    /// objects, or a <see cref="ThreadDispatcher.QueueSynchronizationContext"/>
    /// for objects that live on their own message-loop thread.
    ///
    /// All access must happen on the owner thread — this is what makes it safe
    /// without in-object locks, because each container is only ever touched by a
    /// single thread. Use <see cref="RunOnOwner{T}"/> to marshal an operation
    /// onto the owner thread from any other thread; that always goes through the
    /// owner's <see cref="SynchronizationContext.Post"/>, whether the owner is a
    /// worker or the main thread.
    ///
    /// Ownership is not necessarily permanent: it can be handed over at runtime
    /// with <see cref="ChangeOwner"/>. Every <see cref="RunOnOwner{T}"/> takes a
    /// read lock on a small reader/writer gate; <c>ChangeOwner</c> takes the
    /// write lock and atomically swaps the owner once all in-flight calls have
    /// drained (quiescence), so no mutation ever straddles the handover.
    /// </summary>
    public abstract class OwnedObject : IIdentifiable
    {
        private readonly ReaderWriterLockSlim _handoverGate =
            new(LockRecursionPolicy.SupportsRecursion);

        public int Id { get; }

        private volatile SynchronizationContext? _ownerContext;
        private volatile int _ownerThreadId;

        [ThreadStatic] private static int _dispatchDepth;

        /// <summary>The context of the thread that currently owns this object.
        /// Null means the owner thread has no <see cref="SynchronizationContext"/>
        /// (cross-thread marshalling is then impossible).</summary>
        public SynchronizationContext? Owner => _ownerContext;

        /// <param name="owner">
        /// The <see cref="SynchronizationContext"/> of the thread that owns this
        /// object (e.g. from <see cref="ThreadDispatcher.CreateWorkerThread"/>),
        /// or null to own it on the thread that creates it, capturing that
        /// thread's then-current context.
        /// </param>
        protected OwnedObject(int id, SynchronizationContext? owner)
        {
            Id = id;
            if (owner != null)
            {
                _ownerContext = owner;
                _ownerThreadId = OwnerThreadIdOf(owner);
            }
            else
            {
                _ownerContext = SynchronizationContext.Current;
                _ownerThreadId = Thread.CurrentThread.ManagedThreadId;
            }
        }

        /// <summary>
        /// The worker context knows its thread; anything else is owned by the
        /// thread that is making the call (e.g. handing over to main on main).
        /// </summary>
        private static int OwnerThreadIdOf(SynchronizationContext owner) =>
            (owner as ThreadDispatcher.QueueSynchronizationContext)?.ThreadId
            ?? Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        /// Marshals an operation onto this object's owner thread. If the caller
        /// is already on the owner thread it runs inline.
        /// </summary>
        public T RunOnOwner<T>(Func<T> func)
        {
            _handoverGate.EnterReadLock();
            try
            {
                if (Thread.CurrentThread.ManagedThreadId == _ownerThreadId)
                    return DispatchBody(func);

                var context = _ownerContext;
                if (context == null)
                    throw new InvalidOperationException(
                        $"{GetType().Name}:{Id} is owned by a thread without a " +
                        "SynchronizationContext; it cannot be marshalled onto from another thread.");

                return DispatchOnContext(context, () => DispatchBody(func));
            }
            finally
            {
                _handoverGate.ExitReadLock();
            }
        }

        public void RunOnOwner(Action action) => RunOnOwner<object?>(() =>
        {
            action();
            return null;
        });

        /// <summary>
        /// Hands ownership to a different thread, identified by its
        /// <see cref="SynchronizationContext"/>. Blocks until every in-flight
        /// <see cref="RunOnOwner{T}"/> completes, then swaps the owner;
        /// subsequent calls are marshalled onto the new thread's context.
        /// MUST be called on the thread that will become the new owner (i.e. the
        /// new owner's thread), except when handing to a
        /// <see cref="ThreadDispatcher.QueueSynchronizationContext"/>, which
        /// carries its own thread.
        /// </summary>
        public void ChangeOwner(SynchronizationContext newOwner)
        {
            if (newOwner == null)
                throw new ArgumentNullException(nameof(newOwner));

            ThrowIfInsideDispatch();
            _handoverGate.EnterWriteLock();
            try
            {
                _ownerContext = newOwner;
                _ownerThreadId = OwnerThreadIdOf(newOwner);
            }
            finally
            {
                _handoverGate.ExitWriteLock();
            }
        }

        private static T DispatchBody<T>(Func<T> func)
        {
            _dispatchDepth++;
            try
            {
                return func();
            }
            finally
            {
                _dispatchDepth--;
            }
        }

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

        private static void ThrowIfInsideDispatch()
        {
            if (_dispatchDepth > 0)
                throw new InvalidOperationException(
                    "Cannot change the owner of an OwnedObject from within an " +
                    "operation dispatched to it via RunOnOwner; the handover gate " +
                    "would deadlock (the dispatched operation is waiting on the " +
                    "read lock this call would have to out-wait). Hand over from a " +
                    "neutral thread instead.");
        }

        protected void AssertOnOwner()
        {
            if (Thread.CurrentThread.ManagedThreadId != _ownerThreadId)
                throw new InvalidOperationException(
                    $"{GetType().Name}:{Id} is owned by thread #{_ownerThreadId} " +
                    $"({(_ownerContext is ThreadDispatcher.QueueSynchronizationContext q ? $"worker '{q.Name}'" : "main")}), " +
                    $"but was touched on #{Thread.CurrentThread.ManagedThreadId}. Use RunOnOwner to marshal access.");
        }

        public override string ToString() =>
            _ownerContext != null ? $"@{(_ownerContext is ThreadDispatcher.QueueSynchronizationContext q ? q.Name : "Main")}"
                                  : "@unowned";
    }
}