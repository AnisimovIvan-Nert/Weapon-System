using System;
using System.Collections.Generic;
using System.Linq;
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

        public virtual async ValueTask Execute(TContext context, OperationData data)
        {
            foreach (var subject in Subjects)
            {
                var member = Members.FirstOrDefault(member => member.Member == subject);
                await subject.UpstreamOperation(this, context, data, member);
                await subject.DownstreamOperation(this, context, data, member);
            }

            await InnerExecute(context, data);
        }

        protected abstract ValueTask InnerExecute(TContext context, OperationData data);
    }
}