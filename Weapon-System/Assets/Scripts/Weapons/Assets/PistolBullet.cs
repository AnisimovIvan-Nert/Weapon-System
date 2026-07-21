using System.Collections.Generic;
using System.Linq;
using Weapons.Operations;
using Weapons.ProducerConsumer;
using Weapons.Units.Bullets;

namespace Weapons.Assets
{
    public class PistolBullet : IAsset
    {
        public IBulletData Data;
        public IBulletController Controller;
        public List<IEventConsumer> EventConsumers = new();

        public List<IAsset> Children { get; } = new();

        public PistolBullet(IBulletData data, IBulletController controller)
        {
            Data = data;
            Controller = controller;
        }
        
        public IUnit ToUnit(IEventProducer eventProducer, IOperationRunner runner)
        {
            var children = Children.Select(o => o.ToUnit(eventProducer, runner));
            return new Bullet(eventProducer, runner, Data, Controller, EventConsumers, children.ToArray());
        }
    }
}