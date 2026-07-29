using System;
using OperationSystem.Resource;

namespace OperationSystem.Operations
{
    public class AcquireException : Exception
    {
    }
    
    public class AccessException : Exception
    {
    }
    
    public static class OperationContextExtensions
    {
        public static void Acquire<T>(this IOperationContext context, T resource, OperationIdentifier owner)
            where T : IResource
        {
            if (!context.TryAcquire(resource, owner))
                throw new AcquireException();
        }
        
        public static T Access<T>(this IOperationContext context, OperationIdentifier owner)
            where T : IResource
        {
            return context.TryAccess<T>(owner) ?? throw new AccessException();
        }
    }
}