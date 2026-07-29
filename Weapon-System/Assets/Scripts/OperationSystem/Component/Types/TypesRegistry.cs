using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OperationSystem.Component.Types
{
    public static class TypesRegistry<T>
    {
        // ReSharper disable StaticMemberInGenericType
        private static readonly Dictionary<Type, ComponentType> ComponentTypeMap;
        private static readonly Dictionary<ComponentType, Type> TypeMap;
        private static readonly ConcurrentDictionary<(ComponentType to, ComponentType from), bool> InheritanceCache;
        // ReSharper restore StaticMemberInGenericType

        static TypesRegistry()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetSafeTypes)
                .Where(type => typeof(T).IsAssignableFrom(type))
                .OrderBy(type => type.FullName)
                .ToArray();

            ComponentTypeMap = new Dictionary<Type, ComponentType>(types.Length);
            TypeMap = new Dictionary<ComponentType, Type>(types.Length);
            InheritanceCache = new ConcurrentDictionary<(ComponentType to, ComponentType from), bool>();

            for (var i = 0; i < types.Length; i++)
            {
                var componentType = new ComponentType(i);
                ComponentTypeMap[types[i]] = componentType;
                TypeMap[componentType] = types[i];
            }
        }
        
        public static void TriggerConstructor() { }

        public static ComponentType GetComponentType<TType>() where TType : T
        {
            return GetComponentType(typeof(TType));
        }

        public static ComponentType GetComponentType(Type type)
        {
            return ComponentTypeMap.TryGetValue(type, out var componentType) 
                ? componentType
                : throw new ArgumentException($"Type '{type.FullName}' is not registered.");
        }

        public static Type GetType(ComponentType componentType)
        {
            if (TypeMap.TryGetValue(componentType, out var type))
                return type;

            var id = componentType.Id;
            throw new ArgumentOutOfRangeException(nameof(componentType), id, $"Id '{id}' is not registered.");
        }

        public static bool IsAssignableFrom<TType>(ComponentType from) where TType : T
        {
            var to = GetComponentType<TType>();
            if (to == from)
                return true;
            
            var type = GetType(from);
            return InheritanceCache.GetOrAdd((to, from), _ => typeof(TType).IsAssignableFrom(type));
        }

        private static IEnumerable<Type> GetSafeTypes(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
        }
    }
}