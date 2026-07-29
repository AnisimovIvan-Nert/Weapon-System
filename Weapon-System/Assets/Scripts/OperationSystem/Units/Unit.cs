using System;
using OperationSystem.Component;

namespace OperationSystem.Units
{
    public readonly struct Unit : IEquatable<Unit>
    {
        public UnitId Id { get; }
        public ComponentsData ComponentsData { get; }
        
        public Unit(UnitId id, ComponentsData componentsData)
        {
            Id = id;
            ComponentsData = componentsData;
        }
        
        public static bool operator ==(Unit left, Unit right) => left.Equals(right);
        public static bool operator !=(Unit left, Unit right) => !left.Equals(right);

        public bool Equals(Unit other) => Id.Equals(other.Id);
        public override bool Equals(object? obj) => obj is Unit other && Equals(other);
        public override int GetHashCode() => Id.GetHashCode();
    }
}