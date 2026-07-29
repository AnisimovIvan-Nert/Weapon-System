using System;
using System.Runtime.CompilerServices;

namespace ECS
{
    public struct ComponentMask : IEquatable<ComponentMask>
    {
        private ulong _bits;

        public ComponentMask(ulong bits) => _bits = bits;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(int typeId) => (_bits & (1UL << typeId)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains<T>() where T : IComponent => (_bits & ComponentType<T>.MaskBit) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int typeId) => _bits |= 1UL << typeId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>() where T : IComponent => _bits |= ComponentType<T>.MaskBit;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(int typeId) => _bits &= ~(1UL << typeId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>() where T : IComponent => _bits &= ~ComponentType<T>.MaskBit;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask operator &(ComponentMask a, ComponentMask b) => new(a._bits & b._bits);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask operator |(ComponentMask a, ComponentMask b) => new(a._bits | b._bits);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask operator ~(ComponentMask a) => new(~a._bits);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ComponentMask other) => _bits == other._bits;

        public override readonly bool Equals(object obj) => obj is ComponentMask other && Equals(other);
        public override readonly int GetHashCode() => _bits.GetHashCode();
        public static bool operator ==(ComponentMask a, ComponentMask b) => a._bits == b._bits;
        public static bool operator !=(ComponentMask a, ComponentMask b) => a._bits != b._bits;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask FromTypes(params int[] typeIds)
        {
            var mask = new ComponentMask();
            foreach (var id in typeIds) mask.Add(id);
            return mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask FromTypes<T1>() where T1 : IComponent
        {
            return new ComponentMask(ComponentType<T1>.MaskBit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask FromTypes<T1, T2>() where T1 : IComponent where T2 : IComponent
        {
            return new ComponentMask(ComponentType<T1>.MaskBit | ComponentType<T2>.MaskBit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask FromTypes<T1, T2, T3>() where T1 : IComponent where T2 : IComponent where T3 : IComponent
        {
            return new ComponentMask(ComponentType<T1>.MaskBit | ComponentType<T2>.MaskBit | ComponentType<T3>.MaskBit);
        }

        public readonly bool IsEmpty => _bits == 0;
        public readonly ulong RawValue => _bits;
    }
}
