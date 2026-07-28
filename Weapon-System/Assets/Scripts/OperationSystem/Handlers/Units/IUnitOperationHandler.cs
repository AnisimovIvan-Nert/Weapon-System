using System.Collections;
using OperationSystem.Assets;
using OperationSystem.Units;

namespace OperationSystem.Handlers.Units
{
    public interface IUnitOperationHandler : IOperationHandler
    {
        Unit? Unit { get; }
        
        IEnumerator SetAsset(IAsset? asset);
    }
}