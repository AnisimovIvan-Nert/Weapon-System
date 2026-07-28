using System;

namespace OperationSystem.Units
{
    public readonly struct UnitId : IEquatable<UnitId>
    {
        public int Id { get; }
        
        public UnitId(int id)
        {
            Id = id;
        }

        public bool Equals(UnitId other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is UnitId other && Equals(other);
        public override int GetHashCode() => Id;
    }
}