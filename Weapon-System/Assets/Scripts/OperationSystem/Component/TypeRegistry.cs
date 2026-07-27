using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OperationSystem.Components
{
    public readonly struct TypeRegistry<TBase>
        where TBase : class
    {
        private readonly Dictionary<Type, int> _typeToId;
        private readonly Dictionary<int, Type> _idToType;
        private readonly ConcurrentDictionary<int, bool> _inheritanceCache;

        private static string RegistryName => $"{nameof(TypeRegistry<TBase>)}<{typeof(TBase).Name}>";

        private TypeRegistry(bool dummy)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetSafeTypes)
                .Where(type => typeof(TBase).IsAssignableFrom(type))
                .OrderBy(type => type.FullName)
                .ToArray();

            _typeToId = new Dictionary<Type, int>(types.Length);
            _idToType = new Dictionary<int, Type>(types.Length);
            _inheritanceCache = new ConcurrentDictionary<int, bool>();

            for (var i = 0; i < types.Length; i++)
            {
                _typeToId[types[i]] = i;
                _idToType[i] = types[i];
            }
        }

        public static TypeRegistry<TBase> Create() => new(true);

        public int GetId<T>() where T : TBase
        {
            return GetId(typeof(T));
        }

        public int GetId(Type type)
        {
            if (_typeToId.TryGetValue(type, out var id))
                return id;

            throw new ArgumentException($"Type '{type.FullName}' is not registered in {RegistryName}.");
        }

        public Type GetType(int id)
        {
            if (_idToType.TryGetValue(id, out var type))
                return type;

            throw new ArgumentOutOfRangeException(nameof(id), id, $"Id '{id}' is not registered in {RegistryName}.");
        }

        public bool IsAssignableFrom<T>(int id) where T : TBase
        {
            var typeId = GetId<T>();
            if (typeId == id)
                return true;
            
            var type = GetType(id);
            return _inheritanceCache.GetOrAdd(id, _ => typeof(T).IsAssignableFrom(type));
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