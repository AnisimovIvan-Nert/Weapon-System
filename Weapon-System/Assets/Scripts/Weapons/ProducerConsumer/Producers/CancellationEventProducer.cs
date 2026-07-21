using System;
using System.Collections.Generic;

namespace Weapons.ProducerConsumer.Producers
{
    public class CancellationEventProducer : IEventProducer
    {
        private readonly IInput _input;
        
        private readonly List<IProducedEvent> _events = new();

        public CancellationEventProducer(IInput input)
        {
            _input = input;
        }

        public void Update()
        {
            _events.Clear();

            switch (_input.Cancel)
            {
                case IInput.InputState.Activated:
                    _events.Add(new CancellationEvent());
                    break;
                case IInput.InputState.Deactivated:
                case IInput.InputState.None:
                case IInput.InputState.Active:
                case IInput.InputState.Passive:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerable<IProducedEvent> EnumerateEvents() => _events;
    }
    
    public readonly struct CancellationEvent : IProducedEvent
    {
    }
}