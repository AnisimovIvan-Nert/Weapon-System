using System;
using System.Linq;
using System.Threading;

namespace SecondScratch.ThreadSafe.Commands
{
    public class CommandDependencyAggregator : ICommandDependency
    {
        private int _completedDependenciesCounter;

        public CommandDependencyAggregator(params ICommandDependency?[] dependencies)
        {
            foreach (var dependency in dependencies.OfType<ICommandDependency>())
            {
                Interlocked.Increment(ref _completedDependenciesCounter);
                dependency.OnCompleted += OnDependencyComplete;
            }
        }

        public bool IsCompleted => _completedDependenciesCounter <= 0;
        public event Action? OnCompleted;

        private void OnDependencyComplete()
        {
            Interlocked.Decrement(ref _completedDependenciesCounter);

            if (IsCompleted)
                OnCompleted?.Invoke();
        }
    }
}