using System.Collections.Generic;
using OperationSystem.Operations;

namespace OperationSystem.Assets
{
    public interface IAsset
    {
        IEnumerable<IAsset> Children { get; }

        bool TryAddChild(IAsset child);
        bool TryRemoveChild(IAsset child);

        bool TryLock(OperationIdentifier owner);
        void Release(OperationIdentifier owner);
    }
}