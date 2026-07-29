using System;

namespace OperationSystem.Operations
{
    public readonly struct OperationIdentifier : IEquatable<OperationIdentifier>
    {
        public Guid Guid { get; }

        public OperationIdentifier(Guid guid)
        {
            Guid = guid;
        }

        public static OperationIdentifier CreateNew() => new(Guid.NewGuid());

        public static bool operator ==(OperationIdentifier a, OperationIdentifier b) => a.Equals(b);
        public static bool operator !=(OperationIdentifier a, OperationIdentifier b) => !(a == b);

        public bool Equals(OperationIdentifier other) => Guid.Equals(other.Guid);
        public override bool Equals(object? obj) => obj is OperationIdentifier other && Equals(other);
        public override int GetHashCode() => Guid.GetHashCode();
    }
}