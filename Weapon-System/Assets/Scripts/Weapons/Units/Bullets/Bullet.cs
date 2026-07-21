using Weapons.Operations;
using Weapons.ProducerConsumer;

namespace Weapons.Units.Bullets
{
    public interface IBullet : IUnit<IBulletData>
    {
    }

    public class Bullet
        : AbstractUnit<IBulletData>
        , IBullet
    {
        public Bullet(
            IEventProducer eventProducer,
            IOperationRunner runner,
            IBulletData data,
            params IUnit[] children)
            : base(eventProducer, runner, children, data)
        {
        }
    }
}