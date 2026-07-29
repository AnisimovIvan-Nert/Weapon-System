using System.Collections.Generic;
using System.Linq;
using OperationSystem.Component;
using OperationSystem.Containers.Components.Containers.Locks.Keys;

namespace OperationSystem.Containers.Components
{
    public interface IKeysStorage : IComponent
    {
        IList<IKey> Keys { get; }
    }
    
    public class KeysStorage
        : AbstractComponent
        , IKeysStorage
    {
        public IList<IKey> Keys { get; }
        
        public KeysStorage(params IKey[] keys)
        {
            Keys = keys.ToList();
        }
    }
}