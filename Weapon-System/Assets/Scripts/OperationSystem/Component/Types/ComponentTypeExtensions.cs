using System;

namespace OperationSystem.Component.Types
{
    public static class ComponentTypeExtensions
    {
        public static bool IsAssignableFrom<T>(this ComponentType from)
            where T : IComponent
        {
            return ComponentTypesRegistry.IsAssignableFrom<T>(from);
        }
        
        public static Type ToType(this ComponentType componentType)
        {
            return ComponentTypesRegistry.GetType(componentType);
        }
    }
}