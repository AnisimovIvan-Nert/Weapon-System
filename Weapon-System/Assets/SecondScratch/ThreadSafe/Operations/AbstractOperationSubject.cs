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
        protected readonly List<IOperationSubject> Children = new();

        public IOperationSubject? Parent { get; protected set; }
        IReadOnlyCollection<IOperationSubject> IOperationSubject.Children => Children;

        public virtual ValueTask UpstreamOperation<T>(IOperation operation, OperationData<T> data) 
            where T : IOperationContext
        {
            return Parent?.UpstreamOperation(operation, data) ?? new ValueTask(Task.CompletedTask);
        }

        public virtual async ValueTask DownstreamOperation<T>(IOperation operation, OperationData<T> data) 
            where T : IOperationContext
        {
            foreach (var child in Children)
                await child.DownstreamOperation(operation, data);
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
            Children.Add(child);
        }

        public virtual void RemoveChild(IOperationSubject child)
        {
            if (!Contains(child))
                throw new SubjectMissingChildException();
            
            child.SetParent(null);
            Children.Remove(child);
        }

        protected virtual bool Contains(IOperationSubject child)
        {
            return Children.Contains(child);
        }
    }
}