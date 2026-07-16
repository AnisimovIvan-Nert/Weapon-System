using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons.Units.Chambers
{
    public interface IChamber : IUnit<IChamberData, IChamberController, IChamberAnimator>
    {
    }
    
    public class Chamber 
        : AbstractUnit<IChamberData, IChamberController, IChamberAnimator>
        , IChamber
    {
        public Chamber(
            IUserAdapter user,
            IChamberData data, 
            IChamberController controller,
            IChamberAnimator animator,
            IEnumerable<IOperationsRunner> operationsRunners)
            : base(user, data, controller, animator, operationsRunners)
        {
        }
    }
}