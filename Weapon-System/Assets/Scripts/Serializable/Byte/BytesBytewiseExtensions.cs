using System;

namespace Serializable.Byte
{
    public static class BytesBytewiseExtensions
    {
        public static int BytewiseGetInt(this Bytes bytes, int index)
        {
            if (index + sizeof(int) > bytes.Length)
                throw new IndexOutOfRangeException();

            var result = 0;
            for (var i = 0; i < sizeof(int); i++, index++)
                result |= bytes[index] << i * sizeof(int) * 2;
            return result;
        }

        public static void BytewiseSetInt(this ref Bytes bytes, int index, int value)
        {
            if (index + sizeof(int) > bytes.Length)
                throw new IndexOutOfRangeException();

            for (var i = 0; i < sizeof(int); i++, index++)
                bytes[index] = (byte)(value >> i * sizeof(int) * 2);
        }
    }
}