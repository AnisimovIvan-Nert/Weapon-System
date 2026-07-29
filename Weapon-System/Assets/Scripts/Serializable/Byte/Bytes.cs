using System;
using System.Linq;

namespace Serializable.Byte
{
    public readonly unsafe struct Bytes
    {
        public int Length => Data.Length;
        public int IntLength => Data.Length / sizeof(int);

        public byte[] Data { get; }

        public Bytes(int length)
        {
            Data = new byte[length];
        }

        public byte this[int index]
        {
            get
            {
                CheckIndexInRange(index);
                fixed (byte* result = Data)
                    return result[index];
            }
            set
            {
                CheckIndexInRange(index);
                fixed (byte* result = Data)
                    result[index] = value;
            }
        }

        public void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public int GetInt(int index)
        {
            CheckIndexInRange(index * sizeof(int));
            fixed (byte* result = Data)
                return ((int*)result)[index];
        }

        public void SetInt(int index, int value)
        {
            CheckIndexInRange(index * sizeof(int));
            fixed (byte* result = Data)
                ((int*)result)[index] = value;
        }
        
        private void CheckIndexInRange(int index)
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                const int fnvPrime = 16777619;
                return Data.Aggregate((int)2166136261, (current, t) => (current ^ t) * fnvPrime);
            }
        }
    }
}