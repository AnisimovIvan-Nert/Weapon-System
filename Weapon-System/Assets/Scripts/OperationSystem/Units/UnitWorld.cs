using System;
using OperationSystem.Component;
using OperationSystem.Component.Types;

namespace OperationSystem.Units
{
    public readonly struct UnitWorld
    {
        private readonly IComponentArray?[] _componentArrays;

        public UnitRegistry Registry { get; }

        private UnitWorld(UnitRegistry registry, IComponentArray[] componentArrays)
        {
            Registry = registry;
            _componentArrays = componentArrays;
        }

        public static UnitWorld Create()
        {
            var registry = new UnitRegistry();
            
            ComponentType.WarmUp();
            var count = ComponentType.RegisteredTypeCount;
            var componentArrays = new IComponentArray[count];
            for (var i = 0; i < count; i++)
            {
                var type = ComponentType.GetType(i);
                var arrayType = typeof(ComponentArray<>).MakeGenericType(type);
                componentArrays[i] = (IComponentArray)Activator.CreateInstance(arrayType);
            }
            var world = new UnitWorld(registry, componentArrays);
            registry.SetWorld(world);
            return world;
        }
        
        public ComponentArray<T> GetComponents<T>() where T : struct, IComponent
        {
            return _componentArrays[ComponentType<T>.Id] as ComponentArray<T> ?? throw new InvalidOperationException();
        }
        
        public IComponentArray GetComponents(int typeId) => _componentArrays[typeId] ?? throw new InvalidOperationException();
        
        public IComponentArray? TryGetComponents<T>(Unit unit) where T : IComponent
        {
            foreach (var componentType in unit.ComponentMask)
            {
                if (ComponentType.IsAssignableFrom<T>(componentType))
                    return GetComponents(componentType);
            }

            return null;
        }

        public void PullFromAssets(Unit unit)
        {
            foreach (var typeId in unit.ComponentMask)
                GetComponents(typeId).PullFromAssets(unit.Id, Registry);

            foreach (var child in unit.Children)
                PullFromAssets(child);
        }

        public void PushToAssets(Unit unit)
        {
            foreach (var typeId in unit.ComponentMask)
                GetComponents(typeId).PushToAssets(unit.Id, Registry);

            foreach (var child in unit.Children)
                PushToAssets(child);
        }
    }
}