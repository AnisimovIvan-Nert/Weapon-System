using System;
using System.Runtime.CompilerServices;

namespace ECS
{
    public struct ComponentMask : IEquatable<ComponentMask>
    {
        private ulong[] _words;

        private ComponentMask(ulong[] words) => _words = words;
        internal ComponentMask(int singleTypeId)
        {
            var idx = singleTypeId >> 6;
            _words = new ulong[idx + 1];
            _words[idx] = 1UL << (singleTypeId & 0x3F);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureWord(int wordIndex)
        {
            if (_words == null)
                _words = new ulong[wordIndex + 1];
            else if (_words.Length <= wordIndex)
                Array.Resize(ref _words, wordIndex + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(int typeId)
        {
            var idx = typeId >> 6;
            var bit = 1UL << (typeId & 0x3F);
            return _words != null && idx < _words.Length && (_words[idx] & bit) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains<T>() where T : IComponent => Contains(ComponentType<T>.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int typeId)
        {
            var idx = typeId >> 6;
            var bit = 1UL << (typeId & 0x3F);
            EnsureWord(idx);
            _words[idx] |= bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>() where T : IComponent => Add(ComponentType<T>.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(int typeId)
        {
            var idx = typeId >> 6;
            var bit = 1UL << (typeId & 0x3F);
            if (_words != null && idx < _words.Length)
                _words[idx] &= ~bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>() where T : IComponent => Remove(ComponentType<T>.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask operator &(ComponentMask a, ComponentMask b)
        {
            var aLen = a._words?.Length ?? 0;
            var bLen = b._words?.Length ?? 0;
            var len = Math.Min(aLen, bLen);
            if (len == 0) return default;
            var result = new ulong[len];
            for (int i = 0; i < len; i++)
                result[i] = a._words[i] & b._words[i];
            return new ComponentMask(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask operator |(ComponentMask a, ComponentMask b)
        {
            var aLen = a._words?.Length ?? 0;
            var bLen = b._words?.Length ?? 0;
            var len = Math.Max(aLen, bLen);
            if (len == 0) return default;
            var result = new ulong[len];
            for (int i = 0; i < len; i++)
                result[i] = (i < aLen ? a._words[i] : 0UL)
                          | (i < bLen ? b._words[i] : 0UL);
            return new ComponentMask(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask operator ~(ComponentMask a)
        {
            var words = a._words;
            if (words == null) return default;
            var result = new ulong[words.Length];
            for (int i = 0; i < words.Length; i++)
                result[i] = ~words[i];
            return new ComponentMask(result);
        }

        public readonly bool IsEmpty
        {
            get
            {
                if (_words == null) return true;
                for (int i = 0; i < _words.Length; i++)
                    if (_words[i] != 0) return false;
                return true;
            }
        }

        internal ulong[] GetWords() => _words ?? Array.Empty<ulong>();

        public readonly bool Equals(ComponentMask other)
        {
            var aLen = _words?.Length ?? 0;
            var bLen = other._words?.Length ?? 0;
            var max = Math.Max(aLen, bLen);
            for (int i = 0; i < max; i++)
            {
                var aVal = i < aLen ? _words[i] : 0UL;
                var bVal = i < bLen ? other._words[i] : 0UL;
                if (aVal != bVal) return false;
            }
            return true;
        }

        public override readonly bool Equals(object obj) => obj is ComponentMask other && Equals(other);
        public override readonly int GetHashCode()
        {
            if (_words == null) return 0;
            var hc = 0;
            for (int i = 0; i < _words.Length; i++)
                hc ^= _words[i].GetHashCode();
            return hc;
        }

        public static bool operator ==(ComponentMask a, ComponentMask b) => a.Equals(b);
        public static bool operator !=(ComponentMask a, ComponentMask b) => !a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask FromTypes(params int[] typeIds)
        {
            var mask = new ComponentMask();
            foreach (var id in typeIds) mask.Add(id);
            return mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask FromTypes<T1>() where T1 : IComponent
        {
            return new ComponentMask(ComponentType<T1>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask FromTypes<T1, T2>()
            where T1 : IComponent where T2 : IComponent
        {
            var id1 = ComponentType<T1>.Id;
            var id2 = ComponentType<T2>.Id;
            var max = Math.Max(id1, id2);
            var words = new ulong[(max >> 6) + 1];
            words[id1 >> 6] |= 1UL << (id1 & 0x3F);
            words[id2 >> 6] |= 1UL << (id2 & 0x3F);
            return new ComponentMask(words);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentMask FromTypes<T1, T2, T3>()
            where T1 : IComponent where T2 : IComponent where T3 : IComponent
        {
            var id1 = ComponentType<T1>.Id;
            var id2 = ComponentType<T2>.Id;
            var id3 = ComponentType<T3>.Id;
            var max = Math.Max(Math.Max(id1, id2), id3);
            var words = new ulong[(max >> 6) + 1];
            words[id1 >> 6] |= 1UL << (id1 & 0x3F);
            words[id2 >> 6] |= 1UL << (id2 & 0x3F);
            words[id3 >> 6] |= 1UL << (id3 & 0x3F);
            return new ComponentMask(words);
        }

        public MaskEnumerator GetEnumerator()
        {
            return new MaskEnumerator(_words ?? Array.Empty<ulong>());
        }

        public struct MaskEnumerator
        {
            private readonly ulong[] _words;
            private int _wordIndex;
            private ulong _currentBits;

            internal MaskEnumerator(ulong[] words)
            {
                _words = words;
                _wordIndex = -1;
                _currentBits = 0;
            }

            public bool MoveNext()
            {
                while (_currentBits == 0)
                {
                    _wordIndex++;
                    if (_wordIndex >= _words.Length) return false;
                    _currentBits = _words[_wordIndex];
                }
                return true;
            }

            public int Current
            {
                get
                {
                    var tz = Shared.BitOperations.TrailingZeroCount(_currentBits);
                    _currentBits &= _currentBits - 1;
                    return (_wordIndex << 6) | tz;
                }
            }
        }
    }
}
