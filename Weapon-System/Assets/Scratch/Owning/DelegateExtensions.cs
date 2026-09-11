using System;
using System.Threading.Tasks;

namespace Scratch.Owning
{
    public static class DelegateExtensions
    {
        public static (Action, Task) WrapWithTask(
            this Action action,
            TaskCreationOptions options = TaskCreationOptions.RunContinuationsAsynchronously)
        {
            var tcs = new TaskCompletionSource<object?>(options);
            return (WrappedAction, tcs.Task);
            
            void WrappedAction()
            {
                try
                {
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }
        }
        
        public static (Action, Task<T>) WrapWithTask<T>(
            this Func<T> func,
            TaskCreationOptions options = TaskCreationOptions.RunContinuationsAsynchronously)
        {
            var tcs = new TaskCompletionSource<T>(options);
            return (WrappedAction, tcs.Task);
            
            void WrappedAction()
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }
        }
    }
}