using System.Collections.Generic;
using System.Linq;
using OperationSystem.Containers.Units.Containers.Locks.Keys;
using OperationSystem.Units;

namespace OperationSystem.Containers.Units
{
    public interface IKeysStorage : IUnit
    {
        IList<IKey> Keys { get; }
    }
    
    public class KeysStorage
        : AbstractUnit
        , IKeysStorage
    {
        public IList<IKey> Keys { get; }
        
        public KeysStorage(params IKey[] keys) : base(Enumerable.Empty<IUnit>())
        {
            Keys = keys.ToList();
        }
    }
}