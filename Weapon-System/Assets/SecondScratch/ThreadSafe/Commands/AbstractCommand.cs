using System;

namespace SecondScratch.ThreadSafe.Commands
{
    public class CommandInvalidExecuteException : Exception
    {
    }
    
    public abstract class AbstractCommand<T> : ICommand
        where T : notnull
    {
        private readonly ICommandDependency? _dependency;
        protected readonly T TypedTarget;

        public object Target => TypedTarget;
        public bool IsCompleted { get; private set; }
        
        public event Action? OnReadyForExecute;
        public event Action? OnCompleted;

        public AbstractCommand(T target, ICommandDependency? dependency = null)
        {
            TypedTarget = target;
            _dependency = dependency;

            if (dependency != null)
                dependency.OnCompleted += OnDependencyComplete;
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

        public virtual bool CanExecute() => _dependency?.IsCompleted ?? true;

        protected abstract void InnerExecute();

        protected virtual void OnDependencyComplete() => OnReadyForExecute?.Invoke();
    }
}