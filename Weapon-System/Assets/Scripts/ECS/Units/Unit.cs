using System;
using System.Runtime.CompilerServices;

namespace ECS.Units
{
    public readonly struct Unit : IEquatable<Unit>
    {
        public readonly UnitId Id;

        public Unit(UnitId id)
        {
            Id = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Unit a, Unit b) => a.Id == b.Id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Unit a, Unit b) => a.Id != b.Id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Unit other) => Id.Equals(other.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is Unit other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Id.GetHashCode();
    }
}