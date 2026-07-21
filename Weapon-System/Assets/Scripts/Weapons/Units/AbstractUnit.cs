using System.Collections.Generic;
using Weapons.ProducerConsumer;

namespace Weapons.Units
{
    public abstract class AbstractUnit<TData, TController, TAnimator> : IUnit<TData, TController, TAnimator>
        where TData : IUnitData
        where TController : IUnitController
        where TAnimator : IUnitAnimator
    {
        protected IEnumerable<IEventConsumer> EventConsumers;
        
        public IEventProducer EventProducer { get; }
        public TData Data { get; }
        public TController Controller { get; }
        public TAnimator Animator { get; }
        
        protected AbstractUnit(
            IEventProducer eventProducer,
            TData data, 
            TController controller,
            TAnimator animator,
            IEnumerable<IEventConsumer> eventConsumers)
        {
            EventProducer = eventProducer;
            Data = data;
            Controller = controller;
            Animator = animator;
            EventConsumers = eventConsumers;
        }
        
        public void Update()
        {
            foreach (var eventConsumer in EventConsumers)
                eventConsumer.Update(this);

            Controller.Update(this);
            Animator.Update(this);
        }
    }
}