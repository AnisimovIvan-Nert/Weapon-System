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
        public static void Acquire<T>(this IOperationContext context, T resource, OperationIdentifier owner)
            where T : IComponentResource
        {
            if (!context.TryAcquire(resource, owner))
                throw new AcquireException();
        }
        
        public static IComponentResource Access<T>(this IOperationContext context, OperationIdentifier owner)
            where T : IComponent
        {
            return context.TryAccess<T>(owner) ?? throw new AccessException();
        }
        
        public static T Read<T>(this IOperationContext context, OperationIdentifier owner)
            where T : IComponent
        {
            return context.TryRead<T>(owner) ?? throw new AccessException();
        }
    }
}