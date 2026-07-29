using System;
using System.Runtime.CompilerServices;
using ECS.Shared;
using ECS.Units;

namespace ECS
{
    public struct DirtyTracker
    {
        private ulong[] _bits;

        public DirtyTracker(int initialCapacity = 64)
        {
            var len = Math.Max(1, (initialCapacity + 63) / 64);
            _bits = new ulong[len];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDirty(UnitId unitId)
        {
            var id = unitId.Id;
            var idx = id >> 6;
            var bit = 1UL << (id & 0x3F);
            if (idx >= _bits.Length)
                Array.Resize(ref _bits, Math.Max(idx + 1, _bits.Length * 2));
            _bits[idx] |= bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDirty(UnitId unitId)
        {
            var id = unitId.Id;
            var idx = id >> 6;
            var bit = 1UL << (id & 0x3F);
            return idx < _bits.Length && (_bits[idx] & bit) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(UnitId unitId)
        {
            var id = unitId.Id;
            var idx = id >> 6;
            if (idx >= _bits.Length)
                return;
            _bits[idx] &= ~(1UL << (id & 0x3F));
        }

        public void ClearAll()
        {
            Array.Clear(_bits, 0, _bits.Length);
        }

        internal void EnsureCapacity(int maxEntityId)
        {
            var idx = maxEntityId >> 6;
            if (idx >= _bits.Length)
                Array.Resize(ref _bits, Math.Max(idx + 1, _bits.Length * 2));
        }

        public DirtyEnumerator GetEnumerator()
        {
            return new DirtyEnumerator(_bits);
        }

        public struct DirtyEnumerator
        {
            private readonly ulong[] _bits;
            private int _currentWord;
            private ulong _currentBits;

            internal DirtyEnumerator(ulong[] bits)
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
                    return new UnitId((_currentWord << 6) + tz);
                }
            }
        }
    }
}