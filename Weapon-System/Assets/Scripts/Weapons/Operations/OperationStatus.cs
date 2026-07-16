using System;

namespace Weapons.Operations
{
    /// <summary>
    /// Leading numbers are status codes (000_***).
    /// Last numbers are extra codes (***_000)
    /// </summary>
    public readonly struct OperationStatus : IEquatable<OperationStatus>
    {
        #region Codes

        //Statuses ({name})
        public static OperationStatus None = 0;

        public static OperationStatus Pending = 100_000;

        public static OperationStatus InProgress =  200_000;
        public static OperationStatus InCancellation = 201_000;

        public static OperationStatus Complete = 300_000;

        public static OperationStatus ReadyForDestroying = 400_000;

        public static OperationStatus Destroying = 500_000;
        
        //Extras (With{name})
        public static OperationStatus WithException = 000_100;

        #endregion

        private readonly int _value;
        
        private OperationStatus(int value)
        {
            _value = value;
        }

        public static implicit operator int(OperationStatus status) => status._value;
        public static implicit operator OperationStatus(int code) => new(code);

        public static bool operator ==(OperationStatus left, OperationStatus right)
        {
            return left._value == right._value;
        }
        
        public static bool operator !=(OperationStatus left, OperationStatus right) => !(left == right);

        /// <summary>
        /// NonComplex and NonComplex => simple equal
        /// <para/>
        /// Complex and NonComplex => true if Complex have equal NonComplex part (same status or same extra)
        /// <para/>
        /// Complex and Complex => false
        /// </summary>
        public static bool operator &(OperationStatus left, OperationStatus right)
        {
            if (left == None || right == None)
                throw new NullReferenceException();
            
            if (left.IsComplex() && right.IsComplex())
                return false;

            if (!left.IsComplex() && !right.IsComplex())
                return left == right;

            var (complex, simple) = left.IsComplex() ? (left, right) : (right, left);

            return simple.HasStatus() 
                ? complex.HasStatus(simple) 
                : complex.HasExtra(simple);

        }
        
        /// <summary>
        /// Replace left paths with right parts if parts is valid
        /// <para/>Example: (InProgress.WithException) | (Complete.___) => (Complete.WithException)
        /// </summary>
        public static OperationStatus operator |(OperationStatus left, OperationStatus right)
        {
            if (right == None)
                return left;
            
            if (left.IsComplex() && right.IsComplex())
                return right;

            var status = right.HasStatus() ? right.GetStatus() : left.GetStatus();
            var extra = right.HasExtra() ? right.GetExtra() : left.GetExtra();
            return status.WithExtra(extra);
        }
        
        public bool Equals(OperationStatus other) => this == other;
        public override bool Equals(object? obj) => obj is OperationStatus other && Equals(other);
        
        public override int GetHashCode() => _value;
    }

    public static class OperationStatusExtensions
    {
        private const int Step = 1_000;
        
        public static OperationStatus WithStatus(this OperationStatus code, OperationStatus status)
        {
            return code.GetExtra() + status.GetStatus();
        }

        public static OperationStatus WithExtra(this OperationStatus code, OperationStatus extra)
            => extra.WithStatus(code);

        public static bool HasStatus(this OperationStatus code, OperationStatus status) => code.GetStatus() == status;
        public static bool HasExtra(this OperationStatus code, OperationStatus extra) => code.GetExtra() == extra;
        
        public static bool IsComplex(this OperationStatus code) => code.HasStatus() && code.HasExtra();
        
        public static bool HasStatus(this OperationStatus code) => code.GetStatus() != 0;
        public static bool HasExtra(this OperationStatus code) => code.GetExtra() != 0;
        
        public static OperationStatus GetStatus(this OperationStatus code) => code / Step * Step;
        public static OperationStatus GetExtra(this OperationStatus code) => code % Step;
    }
}