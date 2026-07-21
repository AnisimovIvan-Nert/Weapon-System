using System.Collections.Generic;
using Weapons.Operations;
using Weapons.ProducerConsumer;

namespace Weapons
{
    public interface IUnit
    {
        IEventProducer EventProducer { get; }
        IOperationRunner OperationRunner { get; }
        
        IUnit? Parent { get; }
        IEnumerable<IUnit> Children { get; }
        
        void Update();
    }
    
    public interface IUnit<out TData> : IUnit
        where TData : IUnitData
    {
        TData Data { get; }
    }
    
    public interface IUnit<out TData, out TController> : IUnit<TData>
        where TData : IUnitData
        where TController : IUnitController
    {
        TController Controller { get; }
    }
}