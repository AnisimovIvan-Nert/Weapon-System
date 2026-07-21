using System;
using Weapons.Operations.Implementations;
using Weapons.ProducerConsumer.Producers;
using Weapons.Units.Weapons;

namespace Weapons.ProducerConsumer.Consumers
{
    public class WeaponTriggerEventConsumer : IEventConsumer
    {
        private readonly IEventProducer _eventProducer;

        private bool _triggerPressed;

        public WeaponTriggerEventConsumer(IEventProducer eventProducer)
        {
            _eventProducer = eventProducer;
        }

        public void Update(IUnit unit)
        {
            if (unit is not IWeapon weaponUnit)
                return;
            
            var isPressed = false;
            var isReleased = false;
            
            foreach (var producedEvent in _eventProducer.EnumerateEvents())
            {
                switch (producedEvent)
                {
                    case WeaponTriggerEvent triggerEvent:
                        if (triggerEvent.IsPressed)
                            isPressed = true;
                        else
                            isReleased = true;
                        break;
                }
            }
            
            _triggerPressed &= !isReleased;
            _triggerPressed |= isPressed;
            
            if (_triggerPressed)
                unit.OperationRunner.RunOperation(new ShotOperation(Guid.NewGuid(), weaponUnit));
            
            _triggerPressed &= !isReleased;
        }
    }
}