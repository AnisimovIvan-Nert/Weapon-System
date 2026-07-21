using Weapons.ProducerConsumer;

namespace Weapons
{
    public interface IUnit
    {
        IEventProducer EventProducer { get; }
        
        void Update();
    }
    
    public interface IUnit<out TData, out TController, out TAnimator> : IUnit
        where TData : IUnitData
        where TController : IUnitController
        where TAnimator : IUnitAnimator
    {
        TData Data { get; }
        TController Controller { get; }
        TAnimator Animator { get; }
    }
}