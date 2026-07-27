using System;

namespace OperationSystem.Components
{
    public class ByteArrayExtensions
    {
        public static class ByteArrayShift
        {
            // Shift a slice [sliceStart, sliceStart+sliceLength) right by n positions
            // Elements outside the slice are pushed out if they overlap
            public static void ShiftRight(byte[] array, int sliceStart, int sliceLength, int n)
            {
                if (n <= 0 || sliceLength <= 0) 
                    return;
                var end = sliceStart + sliceLength;
                if (end + n > array.Length) 
                    throw new ArgumentException("Shift exceeds array bounds");

                // Shift right: copy backwards to avoid overwriting
                Buffer.BlockCopy(array, end - n, array, end, n);
                Buffer.BlockCopy(array, sliceStart, array, sliceStart + n, sliceLength - n);
            }

            // Shift a slice [sliceStart, sliceStart+sliceLength) left by n positions
            // Elements outside the slice are pulled in if they overlap
            public static void ShiftLeft(byte[] array, int sliceStart, int sliceLength, int n)
            {
                if (n <= 0 || sliceLength <= 0) 
                    return;
                if (sliceStart - n < 0)
                    throw new ArgumentException("Shift exceeds array bounds");

                // Shift left: copy forwards to avoid overwriting
                Buffer.BlockCopy(array, sliceStart, array, sliceStart - n, sliceLength - n);
                Buffer.BlockCopy(array, sliceStart + sliceLength - n, array, sliceStart, n);
            }
        }
    }
}