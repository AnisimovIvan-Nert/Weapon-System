using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public interface IOperation : IDisposable
    {
        Guid Id { get; }
        OperationState State { get; }
        
        IEnumerable<IOperationSubject> Subjects { get; }
        IEnumerable<OperationTypes> Types { get; }
        IEnumerable<OperationMember> Members { get; }
        
        bool IsCompleted { get; }
        
        void Cancel();
    }
    
    public interface IOperation<TContext> : IOperation
    {
        ValueTask Execute(ref Transaction transaction, ref TContext context, CancellationToken ct);
    }
}