using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public interface IOperationSubject
    {
        IOperationSubject? Parent { get; }
        IReadOnlyCollection<IOperationSubject> Children { get; }
        
        void AddChild(IOperationSubject child);
        void RemoveChild(IOperationSubject child);
        
        internal void SetParent(IOperationSubject? parent);
        
        ValueTask UpstreamOperation<T>(IOperation operation, T context, OperationData data)
            where T : IOperationContext;
        
        ValueTask DownstreamOperation<T>(IOperation operation, T context, OperationData data)
            where T : IOperationContext;
    }
}