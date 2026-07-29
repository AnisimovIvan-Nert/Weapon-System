using System.Threading;

namespace ECS
{
    public static class ComponentType
    {
        private static int _nextId;
        
        public static int Register()
        {
            return Interlocked.Increment(ref _nextId);
        }
    }

    public static class ComponentType<T> where T : IComponent
    {
        public static readonly int Id;

        static ComponentType()
        {
            Id = ComponentType.Register();
        }
    }
}
