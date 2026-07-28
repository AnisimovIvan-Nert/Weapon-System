using System;
using System.Runtime.InteropServices;

namespace Serializable.Byte
{
    public readonly unsafe ref struct Bytes
    {
        public int Length => Data.Length;
        public int IntLength => Data.Length / sizeof(int);

        public Span<byte> Data { get; }

        public Bytes(Span<byte> data)
        {
            Data = data;
        }

        public static Bytes Create(int length)
        {
            return new Bytes(new byte[length]);
        }

        public byte this[int index]
        {
            get
            {
                CheckIndexInRange(index);
                fixed (byte* result = &MemoryMarshal.GetReference(Data))
                    return result[index];
            }
            set
            {
                CheckIndexInRange(index);
                fixed (byte* result = &MemoryMarshal.GetReference(Data))
                    result[index] = value;
            }
        }

        public int GetInt(int index)
        {
            CheckIndexInRange(index * sizeof(int));
            fixed (byte* result = &MemoryMarshal.GetReference(Data))
                return ((int*)result)[index];
        }

        public void SetInt(int index, int value)
        {
            CheckIndexInRange(index * sizeof(int));
            fixed (byte* result = &MemoryMarshal.GetReference(Data))
                ((int*)result)[index] = value;
        }
        
        private void CheckIndexInRange(int index)
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();
        }
    }
}