using System;
using System.Linq;

namespace ECS.Shared
{
    public readonly struct BitCollection
    {
        private const int WordCapacity = sizeof(ulong) * 8;
        private const int IndexShift = 6;

        private readonly object _lock;
        private readonly ulong[][] _wordsReference;

        private ref ulong[] Words => ref  _wordsReference[0];

        public BitCollection(int initialCapacity = WordCapacity - 1)
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
            EnsureCapacity(index);
            var (wordIndex, wordBit) = ToWord(index);

            lock (_lock)
                Words[wordIndex] |= wordBit;
        }

        public void SetFalse(int index)
        {
            EnsureCapacity(index);
            var (wordIndex, wordBit) = ToWord(index);

            lock (_lock)
                Words[wordIndex] &= ~wordBit;
        }

        public bool Any()
        {
            lock (_lock)
                return Words.Any(word => word != 0);
        }

        private void EnsureCapacity(int index)
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
    }
}