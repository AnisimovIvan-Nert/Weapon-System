using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.LockableObject;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Operations
{
    public class SubjectAlreadyContainsChildException : Exception
    {
    }

    public class SubjectMissingChildException : Exception
    {
    }
    
    public abstract partial class AbstractOperationSubject : AbstractLockableObject, IOperationSubject
    {
        protected readonly List<IOperationSubject> ChildrenList = new();

        public IOperationSubject? Parent { get; protected set; }
        public IReadOnlyCollection<IOperationSubject> Children => ChildrenList;

        public virtual ValueTask UpstreamOperation<T>(IOperation operation, T context, OperationData data) 
            where T : IOperationContext
        {
            return Parent?.UpstreamOperation(operation, context, data) ?? new ValueTask(Task.CompletedTask);
        }

        public virtual async ValueTask DownstreamOperation<T>(IOperation operation, T context, OperationData data) 
            where T : IOperationContext
        {
            foreach (var child in ChildrenList)
                await child.DownstreamOperation(operation, context, data);
        }

        void IOperationSubject.SetParent(IOperationSubject? parent)
        {
            Parent = parent;
        }

        public virtual void AddChild(IOperationSubject child)
        {
            if (Contains(child))
                throw new SubjectAlreadyContainsChildException();
            
            child.SetParent(this);
            ChildrenList.Add(child);
        }

        public virtual void RemoveChild(IOperationSubject child)
        {
            if (!Contains(child))
                throw new SubjectMissingChildException();
            
            child.SetParent(null);
            ChildrenList.Remove(child);
        }

        protected virtual bool Contains(IOperationSubject child)
        {
            return ChildrenList.Contains(child);
        }
    }
}