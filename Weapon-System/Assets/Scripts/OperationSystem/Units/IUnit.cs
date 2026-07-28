using OperationSystem.Component;

namespace OperationSystem.Units
{
    public readonly struct Unit
    {
        public UnitId Id { get; }
        public ComponentsData ComponentsData { get; }
        
        public Unit(UnitId id, ComponentsData componentsData)
        {
            Id = id;
            ComponentsData = componentsData;
        }
    }
}