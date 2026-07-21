using System;

namespace Weapons.Operations
{
    public interface IOperation
    {
        Guid Identifier { get; }
        
        OperationStatus Status { get; }
        public Exception? Exception { get; }
        
        bool Increment();
    }
}