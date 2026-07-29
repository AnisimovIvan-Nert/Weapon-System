using System;

namespace OperationSystem.Component.Types
{
    public readonly struct ComponentType : IEquatable<ComponentType>
    {
        public int Id { get; }
        
        public ComponentType(int id)
        {
            Id = id;
        }

        public static ComponentType Create(Type type)
        {
            return TypesRegistry<IComponent>.GetComponentType(type);
        }
        
        public static ComponentType Create<T>()
            where T : IComponent
        {
            return TypesRegistry<IComponent>.GetComponentType<T>();
        }

        public static bool operator ==(ComponentType value, ComponentType other) => value.Equals(other);
        public static bool operator !=(ComponentType value, ComponentType other) => !(value == other);

        public bool Equals(ComponentType other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is ComponentType other && Equals(other);
        public override int GetHashCode() => Id;
    }
}