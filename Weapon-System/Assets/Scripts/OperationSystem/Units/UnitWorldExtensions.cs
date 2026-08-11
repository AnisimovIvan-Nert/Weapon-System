using OperationSystem.Component;
using OperationSystem.Component.Types;

namespace OperationSystem.Units
{
    public static class UnitWorldExtensions
    {
        public static ComponentArray<T> GetComponentArray<T>(this UnitWorld world) 
            where T : struct, IComponent
        {
            return (ComponentArray<T>)world.GetComponentArray(ComponentType<T>.Id);
        }
        
        public static IComponentArray? TryGetComponentArray<T>(this UnitWorld world, Unit unit) 
            where T : IComponent
        {
            foreach (var componentType in unit.ComponentMask)
            {
                if (ComponentType.IsAssignableFrom<T>(componentType))
                    return world.GetComponentArray(componentType);
            }

            return null;
        }
    }
}