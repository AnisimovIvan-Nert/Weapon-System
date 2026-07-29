using System.Runtime.CompilerServices;

namespace ECS
{
    public static class ComponentType
    {
        private static int _nextId;

        public static int Register() => _nextId++;
    }
    
    public static class ComponentType<T> where T : IComponent
    {
        public static readonly int Id;
        public static readonly ulong MaskBit;

        static ComponentType()
        {
            Id = ComponentType.Register();
            MaskBit = 1UL << Id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSetIn(ulong mask) => (mask & MaskBit) != 0;
    }
}
