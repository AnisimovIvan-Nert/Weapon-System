using System;
using System.Collections.Generic;
using System.Threading;
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
        protected readonly SemaphoreSlim ChildrenSemaphore = new(1, 1);
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
            await ChildrenSemaphore.WaitAsync();

            try
            {
                foreach (var child in ChildrenList)
                    await child.DownstreamOperation(operation, context, data);
            }
            finally
            {
                ChildrenSemaphore.Release();
            }
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
            
            ChildrenSemaphore.Wait();
            try
            {
                ChildrenList.Add(child);
            }
            finally
            {
                ChildrenSemaphore.Release();
            }
        }

        public virtual void RemoveChild(IOperationSubject child)
        {
            if (!Contains(child))
                throw new SubjectMissingChildException();
            
            child.SetParent(null);
            
            ChildrenSemaphore.Wait();
            try
            {
                ChildrenList.Remove(child);
            }
            finally
            {
                ChildrenSemaphore.Release();
            }
        }

        protected virtual bool Contains(IOperationSubject child)
        {
            ChildrenSemaphore.Wait();
            try
            {
                return ChildrenList.Contains(child);
            }
            finally
            {
                ChildrenSemaphore.Release();
            }
        }
    }
}