using OperationSystem.Component;
using OperationSystem.Component.Types;

namespace OperationSystem.Units
{
    public readonly struct UnitWorld
    {
        private readonly IComponentArray[] _componentArrays;

        public UnitRegistry Registry { get; }
        
        public bool IsNull { get; }

        private UnitWorld(UnitRegistry registry, IComponentArray[] componentArrays)
        {
            Registry = registry;
            _componentArrays = componentArrays;
            IsNull = true;
        }

        public static UnitWorld Create()
        {
            var registry = new UnitRegistry();
            
            ComponentType.WarmUp();
            var componentArrays = new IComponentArray[ComponentType.RegisteredTypeCount];
            return new UnitWorld(registry, componentArrays);
        }
        
        public ComponentArray<T> GetComponents<T>() where T : struct, IComponent
        {
            return (ComponentArray<T>)_componentArrays[ComponentType<T>.Id];
        }
        
        public IComponentArray GetComponents(int typeId) => _componentArrays[typeId];
        
        public IComponentArray GetComponents<T>(Unit unit) where T : IComponent
        {
            
        }

        public void PullFromAssets(Unit unit)
        {
            foreach (var typeId in unit.ComponentMask)
                _componentArrays[typeId].PullFromAssets(unit.Id, Registry);

            foreach (var child in unit.Children)
                PullFromAssets(child);
        }

        public void PushToAssets(Unit unit)
        {
            foreach (var typeId in unit.ComponentMask)
                _componentArrays[typeId].PushToAssets(unit.Id, Registry);

            foreach (var child in unit.Children)
                PushToAssets(child);
        }
    }
}