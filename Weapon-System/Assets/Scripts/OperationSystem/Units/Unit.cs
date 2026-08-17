using System;
using OperationSystem.Assets;
using OperationSystem.Component.Types;
namespace OperationSystem.Units
{
    public class UnitDestroyedException : Exception
    {
        public UnitId UnitId { get; }
        
        public UnitDestroyedException(UnitId unitId)
            : base($"Unit with id {unitId} destroyed")
        {
            UnitId = unitId;
        }
    }
    
    public readonly struct Unit : IEquatable<Unit>
    {
        public UnitId Id { get; }
        public ComponentMask ComponentMask { get; }
        public IAsset Asset { get; }
        
        public Unit(UnitId id, ComponentMask componentMask, IAsset asset)
        {
            Id = id;
            ComponentMask = componentMask;
            Asset = asset;
        }
        
        public static bool operator ==(Unit left, Unit right) => left.Equals(right);
        public static bool operator !=(Unit left, Unit right) => !left.Equals(right);

        public bool Equals(Unit other) => Id.Equals(other.Id);
        public override bool Equals(object? obj) => obj is Unit other && Equals(other);
        public override int GetHashCode() => Id.GetHashCode();
    }
}