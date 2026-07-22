using System.Threading.Tasks;

namespace Coroutine.Instructions
{
    public readonly struct WaitForTask : IYieldInstruction
    {
        private readonly Task _task;

        public WaitForTask(Task task)
        {
            _task = task;
        }

        public bool IsDone()
        {
            if (!_task.IsCompleted)
                return false;

            var exception = _task.Exception;
            if (exception != null)
                throw exception;

            return true;
        }
    }
}