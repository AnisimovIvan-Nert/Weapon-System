using System;

namespace OperationSystem.Units
{
    public readonly struct UnitId : IEquatable<UnitId>
    {
        public int Id { get; }
        public int Version { get; }
        
        private UnitId(int id, int version)
        {
            Id = id;
            Version = version;
        }

        public static UnitId Create(int id) => new(id, 0);

        public UnitId CreateNewVersion() => new(Id, Version + 1);

        public bool Equals(UnitId other) => Id == other.Id && Version == other.Version;
        public override bool Equals(object? obj) => obj is UnitId other && Equals(other);
        public override int GetHashCode() => Id + (Version << 16);
    }
}