using System;
using OperationSystem.Component;
using OperationSystem.Units;

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
        public static void Acquire<T>(this IOperationContext context, in UnitId unitId, in OperationIdentifier owner)
            where T : struct, IComponent
        {
            if (!context.TryAcquire<T>(unitId, owner))
                throw new AcquireException();
        }
        
        public static T GetReadOnly<T>(this IOperationContext context, in UnitId unitId, in OperationIdentifier owner)
            where T : struct, IComponent
        {
            return context.TryGetReadOnly<T>(unitId, owner) ?? throw new AccessException();
        }
    }
}