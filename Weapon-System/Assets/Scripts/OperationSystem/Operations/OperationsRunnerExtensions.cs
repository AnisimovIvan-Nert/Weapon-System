using System;
using System.Collections;
using System.Linq;
using Coroutine;

namespace OperationSystem.Operations
{
    public static class OperationsRunnerExtensions
    {
        public static void UpdateUntilComplete(
            this IOperationRunner runner,
            IOperation[] operations,
            int? timeout = null)
        {
            runner.UpdateUntilCompleteEnumerator(operations, timeout)
                .Wait();
        }


        public static void UpdateUntilComplete(
            this IOperationRunner runner,
            IOperation operation,
            int? timeout = null)
        {
            runner.UpdateUntilCompleteEnumerator(new[] { operation }, timeout)
                .Wait();
        }

        private static IEnumerator UpdateUntilCompleteEnumerator(
            this IOperationRunner runner, 
            IOperation[] operations,
            int? timeout = null)
        {
            while (operations.Any(operation => !operation.IsCompleted) && timeout-- is null or > 0)
            {
                runner.Update();
                yield return null;
            }

            if (operations.Any(operation => !operation.IsCompleted))
                throw new TimeoutException();
        }
    }
}