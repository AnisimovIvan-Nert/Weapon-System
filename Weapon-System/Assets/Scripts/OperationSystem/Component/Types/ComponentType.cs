using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace OperationSystem.Component.Types
{
    public static class ComponentType<T> where T : struct, IComponent
    {
        public static readonly int Id;

        static ComponentType()
        {
            Id = ComponentType.Register();
        }
    }
    
    public static class ComponentType
    {
        private static int _nextId = -1;
        private static bool _isWarmedUp;
        private static Type[]? _idToType;

        private static Type[] IdToType => _idToType ?? throw new InvalidOperationException();

        public static int RegisteredTypeCount => _nextId;

        public static Type GetType(int typeId) => IdToType[typeId];

        public static bool IsAssignableFrom<T>(int typeId)
        {
            return typeof(T).IsAssignableFrom(IdToType[typeId]);
        }

        public static int Register()
        {
            return Interlocked.Increment(ref _nextId);
        }

        public static void WarmUp()
        {
            if (_isWarmedUp)
                return;
            
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetSafeTypes)
                .Where(type => type.IsValueType && typeof(IComponent).IsAssignableFrom(type))
                .OrderBy(type => type.FullName)
                .ToArray();
            
            foreach (var type in types)
                RuntimeHelpers.RunClassConstructor(typeof(ComponentType<>).MakeGenericType(type).TypeHandle);

            _idToType = new Type[_nextId + 1];
            for (var i = 0; i < types.Length; i++)
            {
                var closedType = typeof(ComponentType<>).MakeGenericType(types[i]);
                var id = (int)closedType.GetField("Id").GetValue(null);
                _idToType[id] = types[i];
            }

            _isWarmedUp = true;
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
