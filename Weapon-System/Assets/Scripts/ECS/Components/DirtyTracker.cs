using System;
using System.Runtime.CompilerServices;
using ECS.Shared;
using ECS.Units;

namespace ECS
{
    public struct DirtyTracker
    {
        private ulong[] _bits;
        private int _capacity;

        public DirtyTracker(int maxEntities)
        {
            var len = Math.Max(1, (maxEntities + 63) / 64);
            _bits = new ulong[len];
            _capacity = maxEntities;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDirty(UnitId unitId)
        {
            var id = unitId.Id;
            var idx = id >> 6;
            var bit = 1UL << (id & 0x3F);
            _bits[idx] |= bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDirty(UnitId unitId)
        {
            var id = unitId.Id;
            var idx = id >> 6;
            var bit = 1UL << (id & 0x3F);
            return (_bits[idx] & bit) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(UnitId unitId)
        {
            var id = unitId.Id;
            var idx = id >> 6;
            var bit = 1UL << (id & 0x3F);
            _bits[idx] &= ~bit;
        }

        public void ClearAll()
        {
            Array.Clear(_bits, 0, _bits.Length);
        }

        public void Resize(int newMaxEntities)
        {
            var newLen = (newMaxEntities + 63) / 64;
            if (newLen <= _bits.Length) return;
            Array.Resize(ref _bits, newLen);
            _capacity = newMaxEntities;
        }

        public DirtyEnumerator GetEnumerator()
        {
            return new DirtyEnumerator(_bits, _capacity);
        }

        public struct DirtyEnumerator
        {
            private readonly ulong[] _bits;
            private int _currentWord;
            private ulong _currentBits;

            internal DirtyEnumerator(ulong[] bits, int capacity)
            {
                _bits = bits;
                _currentWord = -1;
                _currentBits = 0;
            }

            public bool MoveNext()
            {
                while (_currentBits == 0)
                {
                    _currentWord++;
                    if (_currentWord >= _bits.Length) return false;
                    _currentBits = _bits[_currentWord];
                }

                return true;
            }

            public UnitId Current
            {
                get
                {
                    var tz = BitOperations.TrailingZeroCount(_currentBits);
                    _currentBits &= _currentBits - 1;
                    var id = (_currentWord << 6) + tz;
                    return new UnitId(id);
                }
            }
        }
    }
}
