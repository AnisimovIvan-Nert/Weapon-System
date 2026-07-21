using System.Collections.Generic;

namespace Weapons
{
    public interface IUnitController
    {
    }
    
    public interface IComplexUnitController : IUnitController
    {
        IEnumerable<IOperationController> OperationControllers { get; }
    }
}