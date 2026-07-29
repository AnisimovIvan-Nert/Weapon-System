using System;

namespace OperationSystem.Containers.Components.Containers.Locks.Keys
{
    public interface IKey
    {
    }
    
    public readonly struct Key : IKey
    {
        public Guid Identifier { get; }
        
        public Key(Guid identifier)
        {
            Identifier = identifier;
        }
    }
}