using System;

namespace ShiftableData
{
    public class ShiftableDataBuffer<T>
    {
        public T[] Data { get; private set; }

        public int Size => Data.Length;

        public ShiftableDataBuffer(int size = 0)
        {
            Data = new T[size];
        }

        public void Resize(int newSize)
        {
            if (newSize < 0)
                throw new ArgumentOutOfRangeException();
            
            var copyLength = newSize > Size ? Size : newSize;
            var newData = new T[newSize];
            Array.Copy(Data, 0, newData, 0, copyLength);
            Data = newData;
        }
        
        public void IncreaseSize(int requiredSize)
        {
            var size = Size;
            size = size == 0 ? 4 : size;
            while (size < requiredSize)
                size <<= 1;
            Resize(size);
        }

        public void ShiftRight(int startIndex, int endIndex = -1, int distance = 1, bool dropExtra = true)
        {
            if (distance < 1 || startIndex < 0 || endIndex > Data.Length)
                throw new ArgumentOutOfRangeException();

            if (endIndex != -1 && startIndex > endIndex || startIndex == endIndex)
                throw new ArgumentException();

            var targetIndex = startIndex + distance;

            if (targetIndex >= Data.Length)
                throw new ArgumentException("Shift exceeds array bounds");

            endIndex = endIndex == -1 ? Data.Length : endIndex;
            var length = endIndex - startIndex;
            var extra = targetIndex + length - Data.Length;

            if (!dropExtra && extra > 0)
                throw new InvalidOperationException();

            length = extra > 0 ? length - extra : length;

            if (length <= 0)
                throw new InvalidOperationException();
            
            Array.Copy(Data, startIndex, Data, targetIndex, length);
            Array.Clear(Data, startIndex, distance);
        }
        
        public void ShiftLeft(int startIndex, int endIndex = -1, int distance = 1)
        {
            if (distance < 1 || startIndex < 0 || endIndex > Data.Length)
                throw new ArgumentOutOfRangeException();
            
            if (endIndex != -1 && startIndex > endIndex || startIndex == endIndex)
                throw new ArgumentException();
            
            var targetIndex = startIndex - distance;

            if (targetIndex < 0)
                throw new ArgumentException("Shift exceeds array bounds");

            endIndex = endIndex == -1 ? Data.Length : endIndex;
            var length = endIndex - startIndex;
            
            Array.Copy(Data, startIndex, Data, targetIndex, length);
            Array.Clear(Data, endIndex - distance, distance);
        }
    }
}