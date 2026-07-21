namespace Weapons.ProducerConsumer
{
    public interface IEventConsumer
    {
        void Update(IUnit unit);
    }
}