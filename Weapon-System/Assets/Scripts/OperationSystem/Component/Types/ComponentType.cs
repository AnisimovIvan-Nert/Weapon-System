using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace OperationSystem.Component.Types
{
    public static class ComponentType<T> where T : IComponent
    {
        public static readonly int Id;

        static ComponentType()
        {
            Id = ComponentType.Register();
        }
    }
    
    public static class ComponentType
    {
        private static int _nextId;
        private static bool _isWarmedUp;

        public static int RegisteredTypeCount => _nextId;
            
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
                .Where(type => typeof(IComponent).IsAssignableFrom(type))
                .OrderBy(type => type.FullName)
                .ToArray();
            
            foreach (var type in types)
                RuntimeHelpers.RunClassConstructor(typeof(ComponentType<>).MakeGenericType(type).TypeHandle);

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
