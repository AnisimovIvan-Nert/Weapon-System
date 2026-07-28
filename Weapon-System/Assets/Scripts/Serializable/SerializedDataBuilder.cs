using System;
using Serializable.Byte;

namespace Serializable
{
    public readonly ref struct SerializedDataBuilder
    {
        private readonly Bytes _data;
        
        public int Length { get; }
        public int Capacity => _data.Length;

        private SerializedDataBuilder(Bytes data, int length)
        {
            _data = data;
            Length = length;
        }

        public static SerializedDataBuilder Create(int size)
        {
            var bytes = Bytes.Create(size);
            return new SerializedDataBuilder(bytes, 0);
        }

        public SerializedData Build()
        {
            return new SerializedData(_data, Length);
        }

        public SerializedDataBuilder WithInt32(int value = 0)
        {
            if (Length + sizeof(int) > Capacity)
                throw new IndexOutOfRangeException();

            var length = Length + sizeof(int);
            var data = _data;
            data.BytewiseSetInt(Length, value);
            return new SerializedDataBuilder(data, length);
        }
        
        public SerializedDataBuilder WithFloat(float value = 0)
        {
            return WithInt32(BitConverter.SingleToInt32Bits(value));
        }

        public SerializedDataBuilder Append(SerializedData serializedData, int size = -1)
        {
            if (size == -1)
                size = serializedData.Length;
            
            if (Length + size > Capacity)
                throw new InvalidOperationException();

            serializedData.Bytes.Data.Slice(0, size)
                .CopyTo(_data.Data.Slice(Length, size));

            return new SerializedDataBuilder(_data, Length + size);
        }
    }
}