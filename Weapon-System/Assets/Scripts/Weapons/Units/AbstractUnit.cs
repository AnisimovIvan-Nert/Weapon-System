using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons.Units
{
    public abstract class AbstractUnit<TData, TController, TAnimator> : IUnit<TData, TController, TAnimator>
        where TData : IUnitData
        where TController : IUnitController
        where TAnimator : IUnitAnimator
    {
        public IUserAdapter User { get; }
        public TData Data { get; }
        public TController Controller { get; }
        public TAnimator Animator { get; }
        public IEnumerable<IOperationsRunner> OperationsRunners { get; }
        
        protected AbstractUnit(
            IUserAdapter user,
            TData data, 
            TController controller,
            TAnimator animator,
            IEnumerable<IOperationsRunner> operationsRunners)
        {
            User = user;
            Data = data;
            Controller = controller;
            Animator = animator;
            OperationsRunners = operationsRunners;
        }
        
        public void Update()
        {
            foreach (var operationsRunner in OperationsRunners)
                operationsRunner.Update(this);

            Controller.Update(this);
            Animator.Update(this);
        }
    }
}