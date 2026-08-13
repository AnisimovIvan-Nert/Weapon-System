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
        public static void Acquire<T>(this IOperationContext context, in Unit unit, in OperationIdentifier owner)
            where T : struct, IComponent
        {
            if (!context.TryAcquire<T>(unit, owner))
                throw new AcquireException();
        }
        
        public static void Acquire(
            this IOperationContext context, 
            int typeId, 
            in Unit unit, 
            in OperationIdentifier owner)
        {
            if (!context.TryAcquire(typeId, unit, owner))
                throw new AcquireException();
        }
    }
}