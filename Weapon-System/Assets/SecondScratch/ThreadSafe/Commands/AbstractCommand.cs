using System;
using SecondScratch.Shared;
using SecondScratch.ThreadSafe.LockableObject;

namespace SecondScratch.ThreadSafe.Commands
{
    public class CommandInvalidExecuteException : Exception
    {
    }
    
    public abstract class AbstractCommand<T> : ICommand
        where T : ILockableObject
    {
        private readonly ICommandDependency? _dependency;
        protected readonly T TypedTarget;

        public ILockableObject Target => TypedTarget;
        public bool IsCompleted { get; private set; }
        
        public ConditionalEvent? OnReadyForExecute { get; }
        public event Action? OnCompleted;

        protected AbstractCommand(T target, ICommandDependency? dependency = null, bool? canExecute = null)
        {
            TypedTarget = target;
            _dependency = dependency;

            if (dependency != null)
                dependency.OnCompleted += OnDependencyComplete;
            
            canExecute ??= IsDependencyCompleted();
            if (!canExecute.Value)
                OnReadyForExecute = new ConditionalEvent(CanExecute);
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new CommandInvalidExecuteException();
            
            try
            {
                InnerExecute();
            }
            finally
            {
                IsCompleted = true;
                OnCompleted?.Invoke();
            }
        }

        public virtual bool CanExecute() => IsDependencyCompleted();

        protected abstract void InnerExecute();

        protected virtual void OnDependencyComplete()
        {
            OnReadyForExecute?.Invoke();
        }

        private bool IsDependencyCompleted() => _dependency?.IsCompleted ?? true;
    }
}