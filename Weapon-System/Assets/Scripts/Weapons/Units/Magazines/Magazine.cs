using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons.Units.Magazines
{
    public interface IMagazine : IUnit<IMagazine, IMagazineData, IMagazineController, IMagazineAnimator>
    {
        int MagazineNumber { get; set; }
    }
    
    public class Magazine : IMagazine
    {
        public int MagazineNumber { get; set; }
        public IUserAdapter User { get; }
        public IMagazineData Data { get; }
        public IMagazineController Controller { get; }
        public IMagazineAnimator Animator { get; }
        
        public IEnumerable<IOperationsRunner<IMagazine>> OperationsRunners { get; }

        public Magazine(
            IUserAdapter user,
            IMagazineData data, 
            IMagazineController controller,
            IMagazineAnimator animator,
            IEnumerable<IOperationsRunner<IMagazine>> operationsRunners)
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