using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECS.Shared
{
    public readonly struct BitsCollection
    {
        private const int WordCapacity = sizeof(ulong) * 8;
        private const int IndexShift = 6;

        private readonly object _lock;
        private readonly ulong[][] _wordsReference;

        private ref ulong[] Words => ref  _wordsReference[0];

        public BitsCollection(int initialCapacity = WordCapacity - 1)
        {
            var length = initialCapacity / WordCapacity + 1;
            _wordsReference = new []{new ulong[length]};
            _lock = new object();
        }

        public bool IsTrue(int index)
        {
            var (wordIndex, wordBit) = ToWord(index);

            lock (_lock)
            {
                if (wordIndex >= Words.Length)
                    return false;

                return (Words[wordIndex] & wordBit) != 0;
            }
        }

        public void SetTrue(int index)
        {
            EnsureIndexInRange(index);
            var (wordIndex, wordBit) = ToWord(index);

            lock (_lock)
                Words[wordIndex] |= wordBit;
        }

        public void SetFalse(int index)
        {
            EnsureIndexInRange(index);
            var (wordIndex, wordBit) = ToWord(index);

            lock (_lock)
                Words[wordIndex] &= ~wordBit;
        }

        public void Clear()
        {
            lock (_lock)
            {
                for (var i = 0; i < Words.Length; i++)
                    Words[i] = 0;
            }
        }

        public bool Any()
        {
            lock (_lock)
                return Words.Any(word => word != 0);
        }

        public void EnsureIndexInRange(int index)
        {
            lock (_lock)
            {
                if (Words.Length > index >> IndexShift)
                    return;

                Array.Resize(ref Words, index + 1);
            }
        }

        private static (int wordIndex, ulong wordBit) ToWord(int index)
        {
            var wordIndex = index >> IndexShift;
            var wordBit = 1UL << (index & WordCapacity - 1);
            return (wordIndex, wordBit);
        }
        
        public TrueBitsEnumerator GetEnumerator()
        {
            return new TrueBitsEnumerator(Words);
        }
        
        public struct TrueBitsEnumerator : IEnumerator<int>
        {
            private readonly ulong[] _words;
            private int _currentWord;
            private ulong _currentBits;

            internal TrueBitsEnumerator(ulong[] words)
            {
                _words = words;
                _currentWord = -1;
                _currentBits = 0;
            }
            
            public void Reset()
            {
                _currentWord = -1;
                _currentBits = 0;
            }

            public bool MoveNext()
            {
                while (_currentBits == 0)
                {
                    _currentWord++;
                    if (_currentWord >= _words.Length) 
                        return false;
                    _currentBits = _words[_currentWord];
                }

                return true;
            }

            public int Current
            {
                get
                {
                    var trailingZeros = BitOperations.TrailingZeroCount(_currentBits);
                    _currentBits &= _currentBits - 1;
                    return (_currentWord << 6) + trailingZeros;
                }
            }
            
            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}