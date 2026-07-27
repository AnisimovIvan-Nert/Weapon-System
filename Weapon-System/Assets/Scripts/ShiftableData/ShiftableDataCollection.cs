using System;
using System.Collections.Generic;
using System.Linq;

namespace ShiftableData
{
    public readonly struct ShiftableDataCollection<T>
    {
        private readonly ShiftableDataBuffer<T> _data;
        private readonly int[] _lengthData;

        public IReadOnlyList<T> Data => _data.Data;

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

            Array.Copy(data, 0, _data.Data, index, data.Length);
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
            _data.ShiftLeft(endIndex, -1, length);
            Length -= length;
        }
        
        public void Add(params T[] data) => Insert(Length, data);

        public void Replace(int startIndex = 0, int endIndex = -1, params T[] data)
        {
            var end = endIndex == -1 ? Length : endIndex;
            var length = end - startIndex;
            var difference =  data.Length - length; 
            
            switch (difference)
            {
                case < 0:
                    Remove(end + difference, end);
                    break;
                case > 0:
                    var defaultData = Enumerable.Repeat<T>(default!, difference).ToArray();
                    Insert(startIndex + length, defaultData);
                    break;
            }
            
            Array.Copy(data, 0, _data.Data, startIndex, data.Length);
        }

        public Span<T> GetRange(int start, int end) => new(_data.Data, start, end - start);
    }
}