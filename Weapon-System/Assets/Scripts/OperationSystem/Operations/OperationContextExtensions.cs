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
        public static void Acquire(this IOperationContext context, IResource resource, object owner)
        {
            if (!context.TryAcquire(resource, owner))
                throw new AcquireException();
        }
        
        public static T AccessFirst<T>(this IOperationContext context, object owner)
            where T : IResource
        {
            return context.TryAccessFirst<T>(owner) ?? throw new AccessException();
        }
    }
}