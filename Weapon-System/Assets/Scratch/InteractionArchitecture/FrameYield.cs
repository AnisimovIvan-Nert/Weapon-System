using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture
{
    /// <summary>
    /// Provides a frame-aware <see cref="Func{T}"/> (a <c>yield</c> callback)
    /// whose returned task completes on the next Unity frame.  Used to make
    /// interactions genuinely spread their work across multiple frames.
    ///
    /// Attach one of these to a scene GameObject and pump it once per frame via
    /// <see cref="Tick"/>.  Each frame all outstanding yield tasks are completed.
    /// </summary>
    public sealed class FrameYield : IDisposable
    {
        private readonly ConcurrentQueue<TaskCompletionSource<bool>> _pending = new();

        /// <summary>
        /// Returns a <c>yield</c> callback for use with
        /// <see cref="InteractionWorld(Func{Task})"/>.  Awaiting the returned
        /// task suspends until the next <see cref="Tick"/>.
        /// </summary>
        public Func<Task> CreateYield()
        {
            return () =>
            {
                var tcs = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                _pending.Enqueue(tcs);
                return tcs.Task;
            };
        }

        /// <summary>Completes all currently pending yield tasks. Call once per frame.</summary>
        public void Tick()
        {
            while (_pending.TryDequeue(out var tcs))
                tcs.TrySetResult(true);
        }

        public void Dispose()
        {
            while (_pending.TryDequeue(out var tcs))
                tcs.TrySetCanceled();
        }
    }
}
