using System.Collections.Generic;
using Weapons.ProducerConsumer;

namespace Weapons.Units.Weapons
{
    public interface IWeapon : IUnit<IWeaponData, IWeaponController, IWeaponAnimator>
    {
    }

    public class Weapon
        : AbstractUnit<IWeaponData, IWeaponController, IWeaponAnimator>
        , IWeapon
    {
        public Weapon(
            IEventProducer eventProducer,
            IWeaponData data,
            IWeaponController controller,
            IWeaponAnimator animator,
            IEnumerable<IEventConsumer> eventConsumers)
            : base(eventProducer, data, controller, animator, eventConsumers)
        {
        }
    }
}