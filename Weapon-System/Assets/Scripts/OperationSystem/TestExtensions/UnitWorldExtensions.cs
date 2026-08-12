using System;
using System.Collections;
using System.Linq;
using Coroutine;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.TestExtensions
{
    public static class UnitWorldExtensions
    {
        public static void UpdateUntilComplete(
            this UnitWorld world,
            IOperation[] operations,
            int? timeout = null)
        {
            world.UpdateUntilCompleteEnumerator(operations, timeout)
                .Wait();
        }


        public static void UpdateUntilComplete(
            this UnitWorld world,
            IOperation operation,
            int? timeout = null)
        {
            world.UpdateUntilCompleteEnumerator(new[] { operation }, timeout)
                .Wait();
        }

        private static IEnumerator UpdateUntilCompleteEnumerator(
            this UnitWorld world, 
            IOperation[] operations,
            int? timeout = null)
        {
            while (operations.Any(operation => !operation.IsCompleted) && timeout-- is null or > 0)
            {
                world.Update();
                yield return null;
            }

            if (operations.Any(operation => !operation.IsCompleted))
                throw new TimeoutException();
        }
    }
}