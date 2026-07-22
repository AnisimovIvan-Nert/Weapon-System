using System;

namespace OperationSystem.Containers.Units.Containers.Locks.Keys
{
    public interface IKey
    {
    }
    
    public class Key : IKey
    {
        public Guid Identifier { get; }
        
        public Key(Guid identifier)
        {
            Identifier = identifier;
        }
    }
}