using System.Collections.Generic;
using System.Linq;

namespace Weapons.ProducerConsumer
{
    public class EventProducerAggregator : IEventProducer
    {
        private readonly List<IEventProducer> _producers;

        public EventProducerAggregator(params IEventProducer[] producers)
        {
            _producers = producers.ToList();
        }

        public void Update()
        {
            foreach (var producer in _producers)
                producer.Update();
        }

        public IEnumerable<IProducedEvent> EnumerateEvents()
        {
            return _producers.SelectMany(producer => producer.EnumerateEvents());
        }
    }
}