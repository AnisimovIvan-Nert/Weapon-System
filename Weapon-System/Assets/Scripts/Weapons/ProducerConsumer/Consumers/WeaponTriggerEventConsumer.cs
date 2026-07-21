using Weapons.Operations;
using Weapons.Operations.Implementations;
using Weapons.ProducerConsumer.Producers;
using Weapons.Units.Weapons;

namespace Weapons.ProducerConsumer.Consumers
{
    public class WeaponTriggerEventConsumer : IEventConsumer
    {
        private readonly IEventProducer _eventProducer;
        private readonly IOperationRunner _operationRunner;

        private bool _triggerPressed;

        public WeaponTriggerEventConsumer(IEventProducer eventProducer, IOperationRunner operationRunner)
        {
            _eventProducer = eventProducer;
            _operationRunner = operationRunner;
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
                _operationRunner.RunOperation(new ShotOperation(weaponUnit));
            
            _triggerPressed &= !isReleased;
        }
    }
}