using System;
using System.Collections;

namespace Weapons.Operations
{
    public interface IOperation<in T>
        where T : IUnit
    {
        OperationStatus Status { get; }
        public Exception? Exception { get; }
        
        bool Increment(T unit);
    }
}