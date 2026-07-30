using OperationSystem.Units;

namespace OperationSystem.Component
{
    public static class ComponentArrayExtensions
    {
        public static T? TryGetComponent<T>(this ComponentArray<T> componentArray, Unit unit)
            where T : struct, IComponent
        {
            if (!componentArray.HasComponent(unit))
                return null;

            return componentArray.GetComponent(unit.Id);
        }
    }
}