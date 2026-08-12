using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using OperationSystem.Operations.Data;
using OperationSystem.Units;

namespace OperationSystem.Operations
{
    public class OperationDataMissingException<T> : Exception
        where T : IOperationData
    {
    }
    
    public static class OperationExtensions
    {
        public static T GetData<T>(this IOperation operation)
            where T : IOperationData
        {
            return operation.TryGetData<T>() ?? throw new OperationDataMissingException<T>();
        }
        
        public static IEnumerator WaitEnumerator(this IOperation operation)
        {
            var operations = new IOperation[] { operation };
            return operations.WaitEnumerator();
        }

        public static IEnumerator WaitEnumerator(this IOperation[] operations)
        {
            while (operations.Any(operation => !operation.IsCompleted))
                yield return null;
        }

        public static void RunOperationOnWorld(
            this IOperation operation,
            UnitWorld world,
            OperationStaging staging = OperationStaging.Auto)
        {
            operation.RunOperation(world.OperationRunner, world, staging);
        }

        public static Task RunOperationAsTask(
            this IOperation operation,
            UnitWorld world,
            int? timeout = null,
            int? delay = null,
            OperationStaging staging = OperationStaging.Auto)
        {
            var operationRunner = new OperationRunner();
            operation.RunOperation(operationRunner, world, staging);
            return Method(operation, operationRunner, timeout, delay);

            async Task Method(IOperation o, IOperationRunner r, int? t, int? d)
            {
                while (!o.IsCompleted || t-- is null or > 0)
                {
                    r.Update();

                    if (d != null)
                        await Task.Delay(d.Value);
                }

                if (!o.IsCompleted)
                    throw new TimeoutException();
            }
        }
    }
}