using System.Collections.Generic;
using Weapons.Operations;
using Weapons.ProducerConsumer;

namespace Weapons.Assets
{
    public interface IAsset
    {
        List<IAsset> Children { get; }

        IUnit ToUnit(IEventProducer eventProducer, IOperationRunner runner);
    }
}