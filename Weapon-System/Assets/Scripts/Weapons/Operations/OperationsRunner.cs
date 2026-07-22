using System;
using System.Collections.Generic;
using System.Linq;

namespace Weapons.Operations
{
    public class OperationRunner : IOperationRunner
    {
        private readonly List<OperationWithContext> _add = new();
        private readonly List<OperationWithContext> _operations = new();

        private IOperationRunner.IDelayer? _runningDelayer;

        public bool AnyRunningOperation => _operations.Any();

        public void Update()
        {
            if (_runningDelayer == null)
            {
                _operations.AddRange(_add);
                _add.Clear();
            }
            
            for (var i = 0; i < _operations.Count; i++)
            {
                var operation = _operations[i].Operation;
                var context = _operations[i].Context;

                operation.Increment(context);
                if (!operation.IsCompleted)
                    continue;
                
                if (operation.IsCompletedSuccessfully)
                    context.Commit();
                else
                    context.Rollback();
                
                context.Dispose();
                
                _operations.RemoveAt(i);
                i--;
            }
        }

        public void RunOperation(IOperation operation)
        {
            var context = new OperationContext();
            _add.Add(new OperationWithContext(operation, context));
        }
        
        public IOperationRunner.IDelayer DelayOperationRunning()
        {
            _runningDelayer = new IOperationRunner.Delayer();
            return _runningDelayer;
        }

        public void ReleaseOperationRunning(IOperationRunner.IDelayer delayer)
        {
            if (_runningDelayer != delayer)
                return;

            _runningDelayer = null;
        }

        private struct OperationWithContext
        {
            public IOperation Operation { get; }
            public IOperationContext Context { get; }
            
            public OperationWithContext(IOperation operation, IOperationContext context)
            {
                Operation = operation;
                Context = context;
            }
        }
    }
}