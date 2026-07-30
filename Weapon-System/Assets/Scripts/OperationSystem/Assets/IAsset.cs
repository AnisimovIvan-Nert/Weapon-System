using System.Collections.Generic;
using OperationSystem.Component.Types;
using OperationSystem.Units;

namespace OperationSystem.Assets
{
    public interface IAsset
    {
        IEnumerable<IAsset> Children { get; }

        void AddChild(IAsset child);
        void RemoveChild(IAsset child);

        ComponentMask GetComponentMask();
    }
}