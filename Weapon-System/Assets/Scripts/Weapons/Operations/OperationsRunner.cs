using System.Collections.Generic;

namespace Weapons.Operations
{
    public class OperationRunner : IOperationRunner
    {
        private readonly List<IOperation> _add = new();
        private readonly List<IOperation> _operations = new();

        public void Update()
        {
            _operations.AddRange(_add);
            _add.Clear();
            
            for (var i = 0; i < _operations.Count; i++)
            {
                var operation = _operations[i];

                operation.Increment();
                if (!operation.IsCompleted)
                    continue;
                
                _operations.RemoveAt(i);
                i--;
            }
        }

        public void RunOperation(IOperation operation)
        {
            _add.Add(operation);
        }
    }
}