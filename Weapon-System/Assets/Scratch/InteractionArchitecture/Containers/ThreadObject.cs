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
    ///
    /// Ownership is not necessarily permanent: it can be handed over at runtime
    /// with <see cref="ChangeOwner(ThreadDispatcher.WorkerThread)"/> or
    /// <see cref="ChangeOwner(SynchronizationContext)"/>. Every
    /// <see cref="RunOnOwner{T}"/> takes a read lock on a small reader/writer
    /// gate; <c>ChangeOwner</c> takes the write lock and atomically swaps the
    /// owner once all in-flight calls have drained (quiescence), so no
    /// mutation ever straddles the handover. When handing over to the main
    /// thread the caller must provide the main <see cref="SynchronizationContext"/>
    /// so foreign threads can keep marshalling to it.
    /// </summary>
    public abstract class ThreadObject : IIdentifiable
    {
        private readonly ReaderWriterLockSlim _handoverGate =
            new(LockRecursionPolicy.SupportsRecursion);

        public int Id { get; }

        private volatile ThreadDispatcher.WorkerThread? _ownerWorker;
        private volatile int _ownerThreadId;
        private SynchronizationContext? _ownerContext;

        [ThreadStatic] private static int _dispatchDepth;

        /// <summary>The worker thread that currently owns this object, or null
        /// when it is owned by the main thread.</summary>
        public ThreadDispatcher.WorkerThread? Owner => _ownerWorker;
        
        protected ThreadObject(int id, ThreadDispatcher.WorkerThread? owner)
        {
            Id = id;
            if (owner != null)
            {
                _ownerWorker = owner;
                _ownerThreadId = owner.ThreadId;
                _ownerContext = null;
            }
            else
            {
                _ownerWorker = null;
                _ownerThreadId = Thread.CurrentThread.ManagedThreadId;
                _ownerContext = SynchronizationContext.Current;
            }
        }
        
        public T RunOnOwner<T>(Func<T> func)
        {
            _handoverGate.EnterReadLock();
            try
            {
                var worker = _ownerWorker;
                if (worker != null)
                    return worker.InvokeSync(() => DispatchBody(func));

                if (Thread.CurrentThread.ManagedThreadId == _ownerThreadId)
                    return DispatchBody(func);
                
                if (_ownerContext == null)
                    throw new InvalidOperationException(
                        $"{GetType().Name}:{Id} is owned by a thread without a " +
                        "SynchronizationContext; it cannot be marshalled onto from another thread.");

                return DispatchOnContext(_ownerContext, () => DispatchBody(func));
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
        /// Transfers ownership to a dedicated worker thread. Blocks until every
        /// in-flight <see cref="RunOnOwner{T}"/> completes, then swaps the owner;
        /// subsequent calls are marshalled onto the new worker.
        /// </summary>
        public void ChangeOwner(ThreadDispatcher.WorkerThread newWorker)
        {
            if (newWorker == null)
                throw new ArgumentNullException(nameof(newWorker));

            ThrowIfInsideDispatch();
            _handoverGate.EnterWriteLock();
            try
            {
                _ownerWorker = newWorker;
                _ownerThreadId = newWorker.ThreadId;
                _ownerContext = null;
            }
            finally
            {
                _handoverGate.ExitWriteLock();
            }
        }

        /// <summary>
        /// Transfers ownership to the main thread. MUST be called on the main
        /// thread (that thread becomes the owner); pass the main thread's
        /// <see cref="SynchronizationContext"/> so foreign threads can marshal
        /// to it afterwards.
        /// </summary>
        public void ChangeOwner(SynchronizationContext mainThreadContext)
        {
            if (mainThreadContext == null)
                throw new ArgumentNullException(nameof(mainThreadContext));

            ThrowIfInsideDispatch();
            _handoverGate.EnterWriteLock();
            try
            {
                _ownerWorker = null;
                _ownerThreadId = Thread.CurrentThread.ManagedThreadId;
                _ownerContext = mainThreadContext;
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
                    "Cannot change the owner of a ThreadObject from within an " +
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
                    $"({(_ownerWorker != null ? $"worker '{_ownerWorker.Name}'" : "main")}), but was touched on " +
                    $"#{Thread.CurrentThread.ManagedThreadId}. Use RunOnOwner to marshal access.");
        }

        public override string ToString() => _ownerWorker != null ? $"@{_ownerWorker.Name}" : "@Main";
    }
}