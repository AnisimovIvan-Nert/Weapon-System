using System.Linq;
using System.Threading;
using SecondScratch.Shared;

namespace SecondScratch.ThreadSafe.Commands
{
    public class CommandDependencyAggregator : ICommandDependency
    {
        private bool _isInitialized;
        private int _completedDependenciesCounter;

        private CommandDependencyAggregator()
        {
            OnCompleted = new ConditionalEvent(() => IsCompleted, false);
        }

        public static CommandDependencyAggregator Create(params ICommandDependency?[] dependencies)
        {
            var aggregator = new CommandDependencyAggregator();
            
            foreach (var dependency in dependencies.OfType<ICommandDependency>())
            {
                if (dependency.IsCompleted)
                    continue;

                Interlocked.Increment(ref aggregator._completedDependenciesCounter);
                dependency.OnCompleted.CallOrSubscribe(aggregator.OnDependencyComplete);
            }

            aggregator._isInitialized = true;
            
            if (aggregator._completedDependenciesCounter <= 0)
                aggregator.OnCompleted.Invoke();
            
            return aggregator;
        }

        public bool IsCompleted => _completedDependenciesCounter <= 0;
        public ConditionalEvent OnCompleted { get; }

        private void OnDependencyComplete()
        {
            Interlocked.Decrement(ref _completedDependenciesCounter);

            if (_isInitialized && IsCompleted)
                OnCompleted.Invoke();
        }
    }
}