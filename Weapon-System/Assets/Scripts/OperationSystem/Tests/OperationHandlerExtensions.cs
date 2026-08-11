using System;
using System.Collections;
using System.Linq;
using Coroutine;
using OperationSystem.Assets;
using OperationSystem.Handlers;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Tests
{
    public static class OperationHandlerExtensions
    {
        public static Unit SetAndReturnUnit(this IOperationHandler handler, IAsset asset, int? timeout = null)
        {
            Unit unit = default;
            handler.SetUnit(asset).GetResult<Unit>(o => unit = o).Wait(timeout);

            if (unit == default)
                throw new InvalidOperationException();

            return unit;
        }
        
        public static void UpdateUntilComplete(
            this IOperationHandler[] handlers,
            IOperation[] operations,
            int? timeout = null)
        {
            UpdateUntilCompleteEnumerator(handlers, operations, timeout)
                .Wait();
        }

        public static void UpdateUntilComplete(
            this IOperationHandler[] handlers,
            IOperation operation,
            int? timeout = null)
        {
            UpdateUntilCompleteEnumerator(handlers, new[] { operation }, timeout)
                .Wait();
        }

        public static void UpdateUntilComplete(
            this IOperationHandler handler,
            IOperation[] operations,
            int? timeout = null)
        {
            UpdateUntilCompleteEnumerator( new[] { handler }, operations, timeout)
                .Wait();
        }


        public static void UpdateUntilComplete(
            this IOperationHandler handler,
            IOperation operation,
            int? timeout = null)
        {
            UpdateUntilCompleteEnumerator( new[] { handler }, new[] { operation }, timeout)
                .Wait();
        }

        private static IEnumerator UpdateUntilCompleteEnumerator(
            IOperationHandler[] handlers, 
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
    }
}