using System.Collections.Generic;
using Weapons.Assets;
using Weapons.ProducerConsumer;

namespace Weapons
{
    public class UnitAggregator : IUnit
    {
        public IEventProducer EventProducer { get; }
        public List<IUnit> Units { get; }
        
        public UnitAggregator(IEventProducer eventProducer, List<IUnit> units)
        {
            EventProducer = eventProducer;
            Units = units;
        }

        public static UnitAggregator Create(IEventProducer eventProducer, IAsset asset)
        {
            var units = new List<IUnit>();

            var assetQueue = new Queue<IAsset>();
            assetQueue.Enqueue(asset);

            while (assetQueue.Count > 0)
            {
                var currentAsset = assetQueue.Dequeue();
                
                foreach (var child in currentAsset.Children)
                    assetQueue.Enqueue(child);

                var unit = currentAsset.ToUnit(eventProducer);
                units.Add(unit);
            }

            return new UnitAggregator(eventProducer, units);
        }
        
        public void Update()
        {
            EventProducer.Update();
            
            foreach (var unit in Units)
                unit.Update();
        }
    }
}