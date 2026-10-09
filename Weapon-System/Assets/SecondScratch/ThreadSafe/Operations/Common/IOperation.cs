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
    }
    
    public interface IOperation<in TContext> : IOperation
        where TContext : IOperationContext
    {
        ValueTask Execute(Transaction transaction, TContext context, CancellationToken ct);
    }
}