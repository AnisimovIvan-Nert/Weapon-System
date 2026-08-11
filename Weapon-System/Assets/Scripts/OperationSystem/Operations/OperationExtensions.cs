using System;
using System.Collections;
using System.Linq;
using OperationSystem.Operations.Data;

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
    }
}