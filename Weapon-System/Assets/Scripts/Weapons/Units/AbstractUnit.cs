using System.Collections.Generic;
using Weapons.Operations;
using Weapons.ProducerConsumer;

namespace Weapons.Units
{
    public abstract class AbstractUnit<TData> : IUnit<TData>
        where TData : IUnitData
    {
        public IEventProducer EventProducer { get; }
        public IOperationRunner OperationRunner { get; }
        public IUnit? Parent { get; }
        public virtual IEnumerable<IUnit> Children { get; }
        public TData Data { get; }
        
        protected AbstractUnit(
            IEventProducer eventProducer,
            IOperationRunner operationRunner,
            IEnumerable<IUnit> children,
            TData data,
            IUnit? parent = null)
        {
            EventProducer = eventProducer;
            OperationRunner = operationRunner;
            Parent = parent;
            Children = children;
            Data = data;
        }
        
        public virtual void Update()
        {
        }
    }
    
    public abstract class AbstractUnit<TData, TController> : AbstractUnit<TData>, IUnit<TData, TController>
        where TData : IUnitData
        where TController : IUnitController
    {
        protected IEnumerable<IEventConsumer> EventConsumers;

        public TController Controller { get; }
        
        protected AbstractUnit(
            IEventProducer eventProducer,
            IOperationRunner operationRunner,
            IEnumerable<IUnit> children,
            TData data, 
            TController controller,
            IEnumerable<IEventConsumer> eventConsumers,
            IUnit? parent = null)
            : base(eventProducer, operationRunner, children, data, parent)
        {
            Controller = controller;
            EventConsumers = eventConsumers;
        }
        
        public override void Update()
        {
            foreach (var eventConsumer in EventConsumers)
                eventConsumer.Update(this);
        }
    }
}