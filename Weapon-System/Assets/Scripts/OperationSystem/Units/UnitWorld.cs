using ECS;
using ECS.Units;
using OperationSystem.Component;

namespace OperationSystem.Units
{
    public readonly struct UnitWorld
    {
        private readonly IComponentArray[] _componentArrays;
        private readonly object _lock;

        public UnitRegistry Registry { get; }

        private UnitWorld(UnitRegistry registry, IComponentArray[] componentArrays)
        {
            Registry = registry;
            _componentArrays = componentArrays;
            _lock = new object();
        }

        public static UnitWorld Create()
        {
            var registry = new UnitRegistry();
            
            ComponentType.WarmUp();
            var componentArrays = new IComponentArray[ComponentType.RegisteredTypeCount];
            return new UnitWorld(registry, componentArrays);
        }
        
        public ComponentArray<T> GetArray<T>() where T : struct, IComponent
        {
            return (ComponentArray<T>)_componentArrays[ComponentType<T>.Id];
        }
        
        public ref T Get<T>(in Unit unit) where T : struct, IComponent
        {
            return ref GetArray<T>().Get(unit.Id);
        }
    }
}