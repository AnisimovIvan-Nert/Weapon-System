using System.Collections.Generic;
using Weapons.Operations;
using Weapons.User;

namespace Weapons.Units.Magazines
{
    public interface IMagazine : IUnit<IMagazineData, IMagazineController, IMagazineAnimator>
    {
    }

    public class Magazine
        : AbstractUnit<IMagazineData, IMagazineController, IMagazineAnimator>
        , IMagazine
    {
        public Magazine(
            IUserAdapter user,
            IMagazineData data,
            IMagazineController controller,
            IMagazineAnimator animator,
            IEnumerable<IOperationsRunner> operationsRunners)
            : base(user, data, controller, animator, operationsRunners)
        {
        }
    }
}