using System.Collections.Generic;
using OperationSystem.Operations.Middleware;

namespace OperationSystem.Operations
{
    public interface IOperationRunner
    {
        IEnumerable<IOperationMiddleware> Middlewares { get; }
        
        void Update();
        void RunOperation(IOperation operation);
    }
}