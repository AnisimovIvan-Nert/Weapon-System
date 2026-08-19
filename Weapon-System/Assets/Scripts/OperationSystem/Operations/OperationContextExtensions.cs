using System;
using OperationSystem.Assets;

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
        public static void Acquire(this IOperationContext context, IAsset asset, in OperationIdentifier owner)
        {
            if (!context.TryAcquire(asset, owner))
                throw new AcquireException();
        }
    }
}