using System.Collections.Generic;
using System.Linq;
using Weapons.Operations;
using Weapons.ProducerConsumer;
using Weapons.Units.Magazines;

namespace Weapons.Assets
{
    public class PistolMagazine : IAsset
    {
        public IMagazineData Data;
        public IMagazineController Controller;
        public List<IEventConsumer> EventConsumers = new();

        public List<IAsset> Children { get; } = new();

        public PistolMagazine(IMagazineData data, IMagazineController controller)
        {
            Data = data;
            Controller = controller;
        }
        
        public IUnit ToUnit(IEventProducer eventProducer, IOperationRunner runner)
        {
            var children = Children.Select(o => o.ToUnit(eventProducer, runner));
            return new Magazine(eventProducer, runner, Data, Controller, EventConsumers, children.ToArray());
        }
    }
}