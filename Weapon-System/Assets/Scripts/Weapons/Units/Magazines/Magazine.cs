using System.Collections.Generic;
using Weapons.Operations;
using Weapons.ProducerConsumer;

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
            IEventProducer eventProducer,
            IMagazineData data,
            IMagazineController controller,
            IMagazineAnimator animator,
            IEnumerable<IEventConsumer> eventConsumers)
            : base(eventProducer, data, controller, animator, eventConsumers)
        {
        }
    }
}