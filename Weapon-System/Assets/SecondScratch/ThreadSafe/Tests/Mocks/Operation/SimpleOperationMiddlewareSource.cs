using System.Collections.Generic;
using System.Linq;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Operation
{
    public class SimpleOperationMiddlewareSource : IOperationMiddlewareSource
    {
        public List<IOperationMiddleware> Middlewares;

        public SimpleOperationMiddlewareSource(params IOperationMiddleware[] middlewares)
        {
            Middlewares = middlewares.ToList();
        }

        public IEnumerable<IOperationMiddleware> GetMiddlewares() => Middlewares;
    }
}