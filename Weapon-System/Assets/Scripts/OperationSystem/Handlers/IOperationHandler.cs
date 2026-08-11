using System.Collections;
using OperationSystem.Assets;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Handlers
{
    public interface IOperationHandler
    {
        UnitWorld World { get; }
        
        IOperationRunner OperationRunner { get; }
        
        void Update();
        IOperationContext CreateContext();
        IEnumerator SetUnit(IAsset? asset);
        void AppendChild(Unit unit);
    }
}