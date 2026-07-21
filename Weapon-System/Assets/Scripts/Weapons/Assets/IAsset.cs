using System.Collections.Generic;
using Weapons.ProducerConsumer;

namespace Weapons.Assets
{
    public interface IAsset
    {
        List<IAsset> Children { get; }

        IUnit ToUnit(IEventProducer eventProducer);
    }
}