using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Operations
{
    public abstract class AbstractOperation<TContext> : IOperation<TContext>
        where TContext : IOperationContext
    {
        public Guid Id { get; } = Guid.NewGuid();
        public OperationState State { get; private set; }
        
        public virtual IEnumerable<IOperationSubject> Subjects { get; }
        public virtual IEnumerable<OperationTypes> Types { get; }
        public virtual IEnumerable<OperationMember> Members { get; }
        
        public bool IsCompleted => State is OperationState.Committed
            or OperationState.RolledBack
            or OperationState.Failed;
        
        protected AbstractOperation(
            IEnumerable<IOperationSubject> subjects,
            IEnumerable<OperationTypes> types, 
            IEnumerable<OperationMember> members)
        {
            Subjects = subjects;
            Types = types;
            Members = members;
        }
        
        public virtual void Dispose()
        {
        }

        public virtual async ValueTask Execute(OperationData<TContext> data)
        {
            foreach (var subject in Subjects)
            {
                await subject.UpstreamOperation(this, data);
                await subject.DownstreamOperation(this, data);
            }

            await InnerExecute(data);
        }

        protected abstract ValueTask InnerExecute(OperationData<TContext> data);
    }
}