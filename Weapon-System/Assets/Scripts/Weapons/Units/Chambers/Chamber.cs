using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons.Units.Chambers
{
    public interface IChamber : IUnit<IChamber, IChamberData, IChamberController, IChamberAnimator>
    {
    }
    
    public class Chamber : IChamber
    {
        public IUserAdapter User { get; }
        public IChamberData Data { get; }
        public IChamberController Controller { get; }
        public IChamberAnimator Animator { get; }
        
        public IEnumerable<IOperationsRunner<IChamber>> OperationsRunners { get; }

        public Chamber(
            IUserAdapter user,
            IChamberData data, 
            IChamberController controller,
            IChamberAnimator animator,
            IEnumerable<IOperationsRunner<IChamber>> operationsRunners)
        {
            User = user;
            Data = data;
            Controller = controller;
            Animator = animator;
            OperationsRunners = operationsRunners;
        }
        
        public void Update()
        {
            User.Update();

            foreach (var operationsRunner in OperationsRunners)
                operationsRunner.Update(this);

            Controller.Update(this);
            Animator.Update(this);
        }
    }
}