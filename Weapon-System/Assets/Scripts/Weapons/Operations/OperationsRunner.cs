using System.Collections.Generic;
using System.Runtime.ExceptionServices;

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
                
                if (operation.Increment())
                    continue;

                HandleResult(operation);
                
                _operations.RemoveAt(i);
                i--;
            }
        }

        public void RunOperation(IOperation operation)
        {
            _add.Add(operation);
        }

        private static void HandleResult(IOperation operation)
        {
            if (operation.Exception != null)
                ExceptionDispatchInfo.Capture(operation.Exception).Throw();
        }
    }
}