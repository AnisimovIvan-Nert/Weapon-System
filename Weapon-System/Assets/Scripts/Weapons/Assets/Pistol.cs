using System.Collections.Generic;
using Weapons.ProducerConsumer;
using Weapons.Units.Weapons;

namespace Weapons.Assets
{
    public class Pistol : IAsset
    {
        public IWeaponData Data;
        public IWeaponController Controller;
        public IWeaponAnimator Animator;
        public List<IEventConsumer> EventConsumers = new();

        public List<IAsset> Children { get; } = new();

        public Pistol(IWeaponData data, IWeaponController controller, IWeaponAnimator animator)
        {
            Data = data;
            Controller = controller;
            Animator = animator;
        }
        
        public IUnit ToUnit(IEventProducer eventProducer)
        {
            return new Weapon(eventProducer, Data, Controller, Animator, EventConsumers);
        }
    }
}