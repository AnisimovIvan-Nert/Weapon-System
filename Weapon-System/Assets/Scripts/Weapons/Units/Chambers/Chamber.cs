using System.Collections.Generic;
using Weapons.ProducerConsumer;

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
            IEventProducer eventProducer,
            IChamberData data, 
            IChamberController controller,
            IChamberAnimator animator,
            IEnumerable<IEventConsumer> eventConsumers)
            : base(eventProducer, data, controller, animator, eventConsumers)
        {
        }
    }
}