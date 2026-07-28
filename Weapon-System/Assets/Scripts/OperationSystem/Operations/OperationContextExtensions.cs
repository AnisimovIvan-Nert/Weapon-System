using System;
using OperationSystem.Component;

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
        public static void Acquire(
            this IOperationContext context,
            ComponentsData.ComponentResource resource, 
            Guid owner)
        {
            if (!context.TryAcquire(resource, owner))
                throw new AcquireException();
        }
        
        public static ComponentsData.ComponentAccess<T> Access<T>(this IOperationContext context, Guid owner)
            where T : IComponent
        {
            return context.TryAccess<T>(owner) ?? throw new AccessException();
        }
        
        public static T Read<T>(this IOperationContext context, Guid owner)
            where T : IComponent
        {
            return context.TryRead<T>(owner) ?? throw new AccessException();
        }
    }
}