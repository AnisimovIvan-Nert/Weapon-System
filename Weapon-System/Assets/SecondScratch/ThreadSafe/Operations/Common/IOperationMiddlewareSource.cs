using System.Collections.Generic;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public interface IOperationMiddlewareSource
    {
        IEnumerable<IOperationMiddleware> GetMiddlewares();
    }
}