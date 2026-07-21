using System.Collections;
using Weapons.Operations;

namespace Weapons
{
    public interface IOperationController
    {
        IEnumerator Handshake(IUnit unit, IOperation operation);
        IEnumerator CancelHandshake(IUnit unit, IOperation operation);
        
        IEnumerator Perform(IUnit unit, IOperation operation);
        IEnumerator CancelPerform(IUnit unit, IOperation operation);
        
        IEnumerator OnSuccess(IUnit unit, IOperation operation);
        IEnumerator OnFailure(IUnit unit, IOperation operation);
    }
}