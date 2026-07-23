using System;
using OperationSystem.Operations.Data;

namespace OperationSystem.Operations
{
    public class OperationDataException : Exception
    {
    }
    
    public static class OperationExtensions
    {
        public static T GetData<T>(this IOperation operation)
            where T : IOperationData
        {
            return operation.TryGetData<T>() ?? throw new OperationDataException();
        }
    }
}