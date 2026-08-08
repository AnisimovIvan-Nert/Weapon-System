using System.Collections;
using OperationSystem.Assets;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Handlers
{
    public interface IOperationHandler
    {
        Unit? OperationUnit { get; }
        
        IOperationRunner OperationRunner { get; }
        
        void Update();
        IOperationContext CreateContext();
        IEnumerator SetUnit(IAsset? asset);
    }
}