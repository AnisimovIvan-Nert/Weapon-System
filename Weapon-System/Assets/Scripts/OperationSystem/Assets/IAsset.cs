using System.Collections.Generic;
using OperationSystem.Component.Types;

namespace OperationSystem.Assets
{
    public interface IAsset
    {
        IEnumerable<IAsset> Children { get; }

        bool TryAddChild(IAsset child);
        bool TryRemoveChild(IAsset child);
        
        ComponentMask GetComponentMask();
    }
}