using System.Collections.Generic;
using System.Linq;
using Weapons.Operations;
using Weapons.ProducerConsumer;
using Weapons.Units.Chambers;

namespace Weapons.Assets
{
    public class PistolChamber : IAsset
    {
        public IChamberData Data;
        public IChamberController Controller;
        public List<IEventConsumer> EventConsumers = new();

        public List<IAsset> Children { get; } = new();

        public PistolChamber(IChamberData data, IChamberController controller)
        {
            Data = data;
            Controller = controller;
        }
        
        public IUnit ToUnit(IEventProducer eventProducer, IOperationRunner runner)
        {
            var children = Children.Select(o => o.ToUnit(eventProducer, runner));
            return new Chamber(eventProducer, runner, Data, Controller, EventConsumers, children.ToArray());
        }
    }
}