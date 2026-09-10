using System;
using System.Collections.Generic;
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
    /// single thread. Use <see cref="RunOnOwnerAsync{T}(Func{Task{T}})"/> to
    /// marshal an operation onto the owner thread from any other thread; that
    /// always goes through the owner's <see cref="SynchronizationContext.Post"/>,
    /// whether the owner is a worker or the main thread.
    ///
    /// Ownership is not necessarily permanent: it can be handed over at runtime
    /// with <see cref="ChangeOwnerAsync"/>. Every <see cref="RunOnOwnerAsync{T}(Func{Task{T}})"/>
    /// holds a read lease on a small async reader/writer gate; <c>ChangeOwnerAsync</c>
    /// takes the write lease and atomically swaps the owner once all in-flight
    /// calls have drained (quiescence), so no mutation ever straddles the handover.
    /// </summary>
    public abstract class OwnedObject : IIdentifiable
    {
        private readonly AsyncReaderWriterLock _gate = new();
        private readonly AsyncLocal<int> _dispatchDepth = new();

        public int Id { get; }

        private volatile OwnerState _owner;

        /// <summary>The context of the thread that currently owns this object.
        /// Null means the owner thread has no <see cref="SynchronizationContext"/>
        /// (cross-thread marshalling is then impossible).</summary>
        public SynchronizationContext? Owner => _owner?.Context;

        /// <summary>Immutable owner snapshot, swapped atomically on handover.</summary>
        private sealed class OwnerState
        {
            public readonly SynchronizationContext? Context;
            public readonly int ThreadId;

            public OwnerState(SynchronizationContext? context, int threadId)
            {
                Context = context;
                ThreadId = threadId;
            }
        }

        /// <param name="owner">
        /// The <see cref="SynchronizationContext"/> of the thread that owns this
        /// object (e.g. from <see cref="ThreadDispatcher.CreateWorkerThread"/>),
        /// or null to own it on the thread that creates it, capturing that
        /// thread's then-current context.
        /// </param>
        protected OwnedObject(int id, SynchronizationContext? owner)
        {
            Id = id;
            _owner = owner != null
                ? new OwnerState(owner, OwnerThreadIdOf(owner))
                : new OwnerState(SynchronizationContext.Current, Thread.CurrentThread.ManagedThreadId);
        }

        /// <summary>
        /// The worker context knows its thread; anything else is owned by the
        /// thread that is making the call (e.g. handing over to main on main).
        /// </summary>
        private static int OwnerThreadIdOf(SynchronizationContext owner) =>
            (owner as ThreadDispatcher.QueueSynchronizationContext)?.ThreadId
            ?? Thread.CurrentThread.ManagedThreadId;

        // ------------------------------------------------------------------ //
        // Async dispatch
        // ------------------------------------------------------------------ //

        public Task<T> RunOnOwnerAsync<T>(Func<T> func) =>
            RunOnOwnerAsync(() => Task.FromResult(func()));

        public Task RunOnOwnerAsync(Action action) =>
            RunOnOwnerAsync<object?>(() => { action(); return null; });

        public Task RunOnOwnerAsync(Func<Task> func) =>
            RunOnOwnerAsync<object?>(async () =>
            {
                await func();
                return null;
            });

        /// <summary>
        /// Marshals an operation onto this object's owner thread. If the caller
        /// is already on the owner thread the operation runs inline; otherwise
        /// it is dispatched through the owner context and awaited here, fully
        /// completing on the owner thread. The read lease is held until the
        /// operation completes, so a <see cref="ChangeOwnerAsync"/> cannot swap
        /// the owner mid-operation.
        /// </summary>
        public async Task<T> RunOnOwnerAsync<T>(Func<Task<T>> func)
        {
            using (await _gate.ReadAsync())
            {
                var owner = _owner;
                if (owner.ThreadId == Thread.CurrentThread.ManagedThreadId)
                    return await DispatchBodyAsync(func);

                var context = owner.Context;
                if (context == null)
                    throw new InvalidOperationException(
                        $"{GetType().Name}:{Id} is owned by a thread without a " +
                        "SynchronizationContext; it cannot be marshalled onto from another thread.");

                var completionSource = new TaskCompletionSource<T>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                context.Post(async _ =>
                {
                    try
                    {
                        completionSource.SetResult(await DispatchBodyAsync(func));
                    }
                    catch (Exception ex)
                    {
                        completionSource.SetException(ex);
                    }
                }, null);

                return await completionSource.Task;
            }
        }

        /// <summary>
        /// Hands ownership to a different thread, identified by its
        /// <see cref="SynchronizationContext"/>. Waits (asynchronously) until
        /// every in-flight <see cref="RunOnOwnerAsync{T}(Func{Task{T}})"/> has
        /// drained, then swaps the owner; subsequent calls are marshalled onto
        /// the new thread's context. MUST be called on the thread that will
        /// become the new owner (i.e. the new owner's thread), except when
        /// handing to a <see cref="ThreadDispatcher.QueueSynchronizationContext"/>,
        /// which carries its own thread.
        /// </summary>
        public async Task ChangeOwnerAsync(SynchronizationContext newOwner)
        {
            if (newOwner == null)
                throw new ArgumentNullException(nameof(newOwner));

            if (_dispatchDepth.Value > 0)
                throw new InvalidOperationException(
                    "Cannot change the owner of an OwnedObject from within an " +
                    "operation dispatched to it via RunOnOwnerAsync; the handover " +
                    "gate would deadlock (the dispatched operation is waiting on " +
                    "the read lease this call would have to out-wait). Hand over " +
                    "from a neutral thread instead.");

            using (await _gate.WriteAsync())
            {
                _owner = new OwnerState(newOwner, OwnerThreadIdOf(newOwner));
            }
        }

        /// <summary>
        /// Marks the current execution flow as "inside a dispatched operation"
        /// for this object, so <see cref="ChangeOwnerAsync"/> can refuse to be
        /// called from within its own dispatch (which would deadlock the gate).
        /// <see cref="AsyncLocal{T}"/> keeps the marker correct across awaits.
        /// </summary>
        private async Task<T> DispatchBodyAsync<T>(Func<Task<T>> func)
        {
            _dispatchDepth.Value++;
            try
            {
                return await func();
            }
            finally
            {
                _dispatchDepth.Value--;
            }
        }

        protected void AssertOnOwner()
        {
            if (Thread.CurrentThread.ManagedThreadId != _owner.ThreadId)
                throw new InvalidOperationException(
                    $"{GetType().Name}:{Id} is owned by thread #{_owner.ThreadId} " +
                    $"({(_owner.Context is ThreadDispatcher.QueueSynchronizationContext q ? $"worker '{q.Name}'" : "main")}), " +
                    $"but was touched on #{Thread.CurrentThread.ManagedThreadId}. Use RunOnOwnerAsync to marshal access.");
        }

        public override string ToString() =>
            _owner.Context != null ? $"@{(_owner.Context is ThreadDispatcher.QueueSynchronizationContext q ? q.Name : "Main")}"
                                   : "@unowned";

        // ------------------------------------------------------------------ //
        // Async reader/writer gate: many concurrent readers (dispatches), one
        // exclusive writer (handover). Unlike ReaderWriterLockSlim it is not
        // thread-affine, so a lease can be held (and released) across awaits.
        // Writer priority: a handover is not starved by a stream of dispatches.
        // ------------------------------------------------------------------ //
        private sealed class AsyncReaderWriterLock
        {
            private readonly object _sync = new();
            private readonly Queue<TaskCompletionSource<bool>> _waitingReaders = new();
            private readonly Queue<TaskCompletionSource<bool>> _waitingWriters = new();
            private int _readers;
            private bool _writerActive;

            public async Task<Lease> ReadAsync()
            {
                var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                lock (_sync)
                {
                    if (!_writerActive && _waitingWriters.Count == 0)
                    {
                        _readers++;
                        return new Lease(this);
                    }
                    _waitingReaders.Enqueue(gate);
                }
                await gate.Task;
                return new Lease(this);
            }

            public async Task<Lease> WriteAsync()
            {
                var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                lock (_sync)
                {
                    if (!_writerActive && _readers == 0)
                    {
                        _writerActive = true;
                        return new Lease(this);
                    }
                    _waitingWriters.Enqueue(gate);
                }
                await gate.Task;
                return new Lease(this);
            }

            private void Release()
            {
                TaskCompletionSource<bool> admittedWriter = null;
                List<TaskCompletionSource<bool>> admittedReaders = null;

                lock (_sync)
                {
                    if (_writerActive)
                        _writerActive = false;
                    else
                        _readers--;

                    if (_readers == 0 && _waitingWriters.Count > 0)
                    {
                        // Hand straight to the head writer once readers drain.
                        admittedWriter = _waitingWriters.Dequeue();
                        _writerActive = true;
                    }
                    else if (_waitingWriters.Count == 0 && _waitingReaders.Count > 0)
                    {
                        // No writer pending — admit every queued reader at once.
                        admittedReaders = new List<TaskCompletionSource<bool>>(_waitingReaders);
                        _waitingReaders.Clear();
                        _readers += admittedReaders.Count;
                    }
                }

                admittedWriter?.TrySetResult(true);
                if (admittedReaders != null)
                    foreach (var reader in admittedReaders)
                        reader.TrySetResult(true);
            }

            public sealed class Lease : IDisposable
            {
                private AsyncReaderWriterLock _owner;

                public Lease(AsyncReaderWriterLock owner) => _owner = owner;

                public void Dispose()
                {
                    var owner = _owner;
                    _owner = null;
                    owner?.Release();
                }
            }
        }
    }
}