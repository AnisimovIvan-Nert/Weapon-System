using System.Threading.Tasks;

namespace Coroutine.Instructions
{
    public readonly struct WaitForValueTask : IYieldInstruction
    {
        private readonly ValueTask _task;

        public WaitForValueTask(ValueTask task)
        {
            _task = task;
        }

        public bool IsDone()
        {
            if (!_task.IsCompleted)
                return false;

            var exception = _task.AsTask().Exception;
            if (exception != null)
                throw exception;

            return true;
        }
    }
}