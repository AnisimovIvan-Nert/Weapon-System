using System;

namespace Weapons.Operations
{
    public interface IOperation
    {
        OperationStatus Status { get; }
        public Exception? Exception { get; }
        
        bool Increment();
    }
}