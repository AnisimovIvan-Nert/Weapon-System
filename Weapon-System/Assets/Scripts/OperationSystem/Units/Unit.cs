using System;
using System.Collections.Generic;
using OperationSystem.Component.Types;
using OperationSystem.Handlers;

namespace OperationSystem.Units
{
    public readonly struct Unit : IEquatable<Unit>
    {
        public UnitId Id { get; }
        public ComponentMask ComponentMask { get; }
        public IList<Unit> Children { get; }
        public UnitWorld World { get; }
        public IOperationHandler OperationHandler { get; }
        
        public Unit(
            UnitId id, 
            ComponentMask componentMask,
            UnitWorld world,
            IOperationHandler operationHandler, 
            params Unit[] children)
        {
            Id = id;
            ComponentMask = componentMask;
            World = world;
            OperationHandler = operationHandler;
            Children = children;
        }
        
        public static bool operator ==(Unit left, Unit right) => left.Equals(right);
        public static bool operator !=(Unit left, Unit right) => !left.Equals(right);

        public bool Equals(Unit other) => Id.Equals(other.Id);
        public override bool Equals(object? obj) => obj is Unit other && Equals(other);
        public override int GetHashCode() => Id.GetHashCode();
    }
}