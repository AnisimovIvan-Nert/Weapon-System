using System;
using System.Collections.Generic;

namespace Weapons.ProducerConsumer.Producers
{
    public class WeaponTriggerEventProducer : IEventProducer
    {
        private readonly IInput _input;
        
        private readonly List<IProducedEvent> _events = new();

        public WeaponTriggerEventProducer(IInput input)
        {
            _input = input;
        }

        public void Update()
        {
            _events.Clear();

            switch (_input.Shoot)
            {
                case IInput.InputState.Activated:
                    _events.Add(new WeaponTriggerEvent(true));
                    break;
                case IInput.InputState.Deactivated:
                    _events.Add(new WeaponTriggerEvent(false));
                    break;
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

    public readonly struct WeaponTriggerEvent : IProducedEvent
    {
        public bool IsPressed { get; }

        public WeaponTriggerEvent(bool isPressed)
        {
            IsPressed = isPressed;
        }
    }
}