using System;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public readonly struct OperationMember : IEquatable<OperationMember>
    {
        public readonly Members Type;
        public readonly object Member;

        public OperationMember(Members type, object member)
        {
            Type = type;
            Member = member;
        }
        
        public static bool operator ==(OperationMember left, OperationMember right) => left.Equals(right);
        public static bool operator !=(OperationMember left, OperationMember right) => !left.Equals(right);

        public bool Equals(OperationMember other) => Type == other.Type && Member.Equals(other.Member);
        public override bool Equals(object? obj) => obj is OperationMember other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Type, Member);
    }

    public enum Members
    {
        UserDefinedTypes = 0b11111111_11111111_0000000_0000000,
        
        Executor = 1,
        Subject = Executor + 1,
        
        Destination = Subject + 1,
        Source = Destination + 1,
    }
}