using System;

namespace OperationSystem.Component.Types
{
    public static class ComponentTypeExtensions
    {
        public static bool IsAssignableFrom<T>(this ComponentType from)
            where T : IComponent
        {
            return TypesRegistry<IComponent>.IsAssignableFrom<T>(from);
        }
        
        public static Type ToType(this ComponentType componentType)
        {
            return TypesRegistry<IComponent>.GetType(componentType);
        }
    }
}