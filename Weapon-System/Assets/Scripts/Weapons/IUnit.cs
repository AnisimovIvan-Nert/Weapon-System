using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons
{
    public interface IUnit
    {
        IUserAdapter User { get; }
        
        void Update();
    }
    
    public interface IUnit<out TData, out TController, out TAnimator> : IUnit
        where TData : IUnitData
        where TController : IUnitController
        where TAnimator : IUnitAnimator
    {
        TData Data { get; }
        TController Controller { get; }
        TAnimator Animator { get; }
        IEnumerable<IOperationsRunner> OperationsRunners { get; }
    }
}