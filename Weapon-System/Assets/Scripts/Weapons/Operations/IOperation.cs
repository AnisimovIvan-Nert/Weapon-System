using System;

namespace Weapons.Operations
{
    public interface IOperation
    {
        Guid Identifier { get; }
        
        bool IsCompleted { get; }
        public Exception? Exception { get; }
        
        void Increment();
    }
}