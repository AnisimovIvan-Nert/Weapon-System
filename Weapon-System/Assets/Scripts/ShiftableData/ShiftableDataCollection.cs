using System;

namespace ShiftableData
{
    public readonly struct ShiftableDataCollection<T>
    {
        private readonly ShiftableDataBuffer<T> _data;
        private readonly int[] _lengthData;

        public T[] Data => _data.Data;

        public int Length
        {
            get => _lengthData[0];
            private set => _lengthData[0] = value;
        }

        public ShiftableDataCollection(int size = 0)
        {
            _data = new ShiftableDataBuffer<T>(size);
            _lengthData = new[] { 0 };
        }

        public void Resize(int newSize)
        {
            _data.Resize(newSize);

            if (newSize < Length)
                Length = newSize;
        }

        public void Insert(int index, params T[] data)
        {
            if (index < 0 || index > Length)
                throw new ArgumentOutOfRangeException();
            
            if (data.Length == 0)
                return;
            
            if (Length + data.Length > _data.Size)
                _data.IncreaseSize(Length + data.Length);
            
            if (index != Length)
                _data.ShiftRight(index, Length, data.Length);

            Array.Copy(data, 0, Data, index, data.Length);
            Length += data.Length;
        }

        public void Remove(int startIndex = 0, int endIndex = -1)
        {
            endIndex = endIndex == -1 ? Length : endIndex;

            if (startIndex < 0 || startIndex > Length 
                               || endIndex < 0 || endIndex > Length)
                throw new ArgumentOutOfRangeException();

            if (endIndex - startIndex < 0)
                throw new ArgumentException();
            
            if (endIndex == startIndex)
                return;

            var length = endIndex - startIndex;
            _data.ShiftLeft(endIndex, Length, endIndex - startIndex);
            Length -= length;
        }
        
        public void Add(params T[] data) => Insert(Length, data);

        public void Replace(int startIndex = 0, int endIndex = -1, params T[] data)
        {
            var end = endIndex == -1 ? Length : endIndex;
            var length = end - startIndex;
            if (length == data.Length)
            {
                Array.Copy(data, 0, Data, startIndex, length);
                return;
            }
            
            Remove(startIndex, endIndex);
            Insert(startIndex, data);
        }
    }
}