namespace SecondScratch.ThreadSafe.Commands
{
    public abstract class AbstractCommand<T> : ICommand
        where T : notnull
    {
        private readonly ICommand? _dependency;
        protected readonly T TypedTarget;

        public object Target => TypedTarget;
        
        public bool IsCompleted { get; private set; }

        public AbstractCommand(T target, ICommand? dependency = null)
        {
            TypedTarget = target;
            _dependency = dependency;
        }

        public void Execute()
        {
            try
            {
                InnerExecute();
            }
            finally
            {
                IsCompleted = true;
            }
        }

        public virtual bool CanExecute() => _dependency?.IsCompleted ?? true;

        protected abstract void InnerExecute();
    }
}