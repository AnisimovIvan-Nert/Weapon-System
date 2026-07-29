using System;
using System.Runtime.CompilerServices;

namespace ECS.Units
{
    public readonly struct UnitId : IEquatable<UnitId>
    {
        public int Id { get; }

        public UnitId(int id)
        {
            Id = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UnitId a, UnitId b) => a.Id == b.Id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnitId a, UnitId b) => a.Id != b.Id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UnitId other) => Id == other.Id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is UnitId other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Id;
    }
}