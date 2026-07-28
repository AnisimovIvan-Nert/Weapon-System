using System;
using Serializable.Byte;

namespace Serializable
{
    public readonly ref struct SerializedData
    {
        public Bytes Bytes { get; }
        public int Length { get; }

        public SerializedData(Bytes bytes, int length)
        {
            if (bytes.Length < length)
                throw new ArgumentException();
            
            Bytes = bytes;
            Length = length;
        }

        public static SerializedData Create(int data)
        {
            return SerializedDataBuilder.Create(sizeof(int))
                .WithInt32(data)
                .Build();
        }

        public int ToInt32()
        {
            SerializedDataReader.Create(this)
                .ReadInt32(out var value);
            return value;
        }
        
        public static SerializedData Create(float data)
        {
            return SerializedDataBuilder.Create(sizeof(float))
                .WithFloat(data)
                .Build();
        }

        public float ToFloat()
        {
            SerializedDataReader.Create(this)
                .ReadFloat(out var value);
            return value;
        }
    }
}