using System;
using System.Collections;
using System.Linq;
using OperationSystem.Handlers;
using OperationSystem.Operations;

namespace OperationSystem.Tests
{
    public static class OperationHandlerExtensions
    {
        public static IEnumerator UpdateUntilComplete(
            this IOperationHandler[] handlers,
            IOperation[] operations,
            int? timeout = null)
        {
            while (operations.Any(operation => !operation.IsCompleted) && timeout-- is null or > 0)
            {
                foreach (var handler in handlers)
                    handler.Update();
                yield return null;
            }

            if (operations.Any(operation => !operation.IsCompleted))
                throw new InvalidOperationException();
        }
        
        public static IEnumerator UpdateUntilComplete(
            this IOperationHandler[] handlers,
            IOperation operation,
            int? timeout = null)
        {
            return handlers.UpdateUntilComplete(new[] { operation }, timeout);
        }
        
        public static IEnumerator UpdateUntilComplete(
            this IOperationHandler handler,
            IOperation[] operations,
            int? timeout = null)
        {
            var handlers = new[] { handler };
            return handlers.UpdateUntilComplete(operations, timeout);
        }

        public static IEnumerator UpdateUntilComplete(
            this IOperationHandler handler,
            IOperation operation,
            int? timeout = null)
        {
            return handler.UpdateUntilComplete(new[] { operation }, timeout);
        }
    }
}