using System;
using Serializable.Byte;

namespace Serializable
{
    public readonly ref struct SerializedDataReader
    {
        private readonly Bytes _data;
        private readonly int _offset;
        
        private SerializedDataReader(Bytes data, int offset)
        {
            _data = data;
            _offset = offset;
        }

        public static SerializedDataReader Create(SerializedData serializedData)
        {
            return new SerializedDataReader(serializedData.Bytes, 0);
        }
        
        public SerializedDataReader ReadInt32(out int value)
        {
            if (_offset + sizeof(int) > _data.Length)
                throw new IndexOutOfRangeException();

            value = _data.BytewiseGetInt(_offset);
            var offset = _offset + sizeof(int);
            return new SerializedDataReader(_data, offset);
        }
        
        public SerializedDataReader ReadFloat(out float value)
        {
            var reader = ReadInt32(out var intValue);
            value = BitConverter.Int32BitsToSingle(intValue);
            return reader;
        }

        public SerializedDataReader ReadSerializedData(out SerializedData value, int size = -1)
        {
            if (size < 0)
                size = _data.Length - _offset;
            
            if (_offset + size > _data.Length)
                throw new IndexOutOfRangeException();

            var data = Bytes.Create(size);
            _data.Data.Slice(_offset, size).CopyTo(data.Data);

            value = new SerializedData(data, size);
            return new SerializedDataReader(_data, _offset + size);
        }
    }
}