using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OperationSystem.Component.Types
{
    public static class ComponentTypesRegistry
    {
        private static readonly Dictionary<Type, ComponentType> FromTypeMap;
        private static readonly Dictionary<ComponentType, Type> ToTypeMap;
        private static readonly ConcurrentDictionary<(ComponentType to, ComponentType from), bool> InheritanceCache;

        static ComponentTypesRegistry()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetSafeTypes)
                .Where(type => typeof(IComponent).IsAssignableFrom(type))
                .OrderBy(type => type.FullName)
                .ToArray();

            FromTypeMap = new Dictionary<Type, ComponentType>(types.Length);
            ToTypeMap = new Dictionary<ComponentType, Type>(types.Length);
            InheritanceCache = new ConcurrentDictionary<(ComponentType to, ComponentType from), bool>();

            for (var i = 0; i < types.Length; i++)
            {
                var componentType = new ComponentType(i);
                FromTypeMap[types[i]] = componentType;
                ToTypeMap[componentType] = types[i];
            }
        }

        public static ComponentType GetComponentType<T>() where T : IComponent
        {
            return GetComponentType(typeof(T));
        }

        public static ComponentType GetComponentType(Type type)
        {
            return FromTypeMap.TryGetValue(type, out var componentType) 
                ? componentType
                : throw new ArgumentException($"Type '{type.FullName}' is not registered.");
        }

        public static Type GetType(ComponentType componentType)
        {
            if (ToTypeMap.TryGetValue(componentType, out var type))
                return type;

            var id = componentType.Id;
            throw new ArgumentOutOfRangeException(nameof(componentType), id, $"Id '{id}' is not registered.");
        }

        public static bool IsAssignableFrom<T>(ComponentType from) where T : IComponent
        {
            var to = GetComponentType<T>();
            if (to == from)
                return true;
            
            var type = GetType(from);
            return InheritanceCache.GetOrAdd((to, from), _ => typeof(T).IsAssignableFrom(type));
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