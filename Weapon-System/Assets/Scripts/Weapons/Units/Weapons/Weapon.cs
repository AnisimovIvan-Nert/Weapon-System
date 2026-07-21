using System.Collections.Generic;
using Weapons.Operations;
using Weapons.ProducerConsumer;
using Weapons.Units.Weapons.Controller;

namespace Weapons.Units.Weapons
{
    public interface IWeapon : IUnit<IWeaponData, IWeaponController>
    {
    }

    public class Weapon
        : AbstractUnit<IWeaponData, IWeaponController>
        , IWeapon
    {
        public Weapon(
            IEventProducer eventProducer,
            IOperationRunner runner,
            IWeaponData data,
            IWeaponController controller,
            IEnumerable<IEventConsumer> eventConsumers,
            params IUnit[] children)
            : base(eventProducer, runner, children, data, controller, eventConsumers)
        {
        }
    }
}