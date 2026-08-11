using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OperationSystem.Operations
{
    public class OperationRunner : IOperationRunner
    {
        private readonly ConcurrentBag<Operation> _pendingAdd = new();
        private readonly HashSet<Operation> _operations = new();

        public bool AnyRunningOperation => _operations.Any();

        public void Update()
        {
            AddPendingOperations();

            foreach (var operation in _operations)
                operation.Increment();

            _operations.RemoveWhere(operation => operation.IsCompleted);
        }

        public void RunOperation(IOperation operation, IOperationContext context)
        {
            _pendingAdd.Add(new Operation(operation, context));
        }

        private void AddPendingOperations()
        {
            while (_pendingAdd.TryTake(out var operation))
            {
                if (!_operations.Add(operation))
                    throw new InvalidOperationException("Operation already running");
            }
        }

        private readonly struct Operation : IEquatable<Operation>
        {
            private readonly IOperation _operation;
            private readonly IOperationContext _context;

            public bool IsCompleted => _operation.IsCompleted;

            public Operation(IOperation operation, IOperationContext context)
            {
                _operation = operation;
                _context = context;
            }

            public void Increment()
            {
                _operation.Increment(_context);
                if (!_operation.IsCompleted)
                    return;
                
                if (_operation.IsCompletedSuccessfully)
                    _context.Commit();
                else
                    _context.Rollback();

                _context.Dispose();
            }

            public static bool operator ==(Operation left, Operation right) => left.Equals(right);
            public static bool operator !=(Operation left, Operation right) => !(left == right);

            public bool Equals(Operation other) => _operation.Equals(other._operation);
            public override bool Equals(object? obj) => obj is Operation other && Equals(other);
            public override int GetHashCode() => _operation.GetHashCode();
        }
    }
}