using System.Collections.Generic;
using OperationSystem.Units;

namespace OperationSystem.Assets
{
    public interface IAsset : IUnitTarget
    {
        IEnumerable<IAsset> Children { get; }

        void AddChild(IAsset child);
        void RemoveChild(IAsset child);
    }
}