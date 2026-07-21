using System.Collections.Generic;

namespace Weapons.ProducerConsumer
{
    public interface IEventProducer
    {
        void Update();
        
        IEnumerable<IProducedEvent> EnumerateEvents();
    }
}