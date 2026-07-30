using System.Collections;
using System.Collections.Generic;
using OperationSystem.Shared;
using OperationSystem.Units;

namespace OperationSystem.Component
{
    public readonly struct DirtyTracker
    {
        private readonly BitsCollection _dirtyBits;

        public DirtyTracker(int initialCapacity = 63)
        {
            _dirtyBits = BitsCollection.Create(initialCapacity);
        }

        public void SetDirty(int index) => _dirtyBits.SetTrue(index);
        public bool IsDirty(int index) => _dirtyBits.IsTrue(index);
        public void Clear(int index) => _dirtyBits.SetFalse(index);
        public void ClearAll() => _dirtyBits.Clear();
        public void EnsureCapacity(int capacity) => _dirtyBits.EnsureIndexInRange(capacity - 1);

        public DirtyEnumerator GetEnumerator() => new(_dirtyBits);

        public struct DirtyEnumerator : IEnumerator<UnitId>
        {
            private BitsCollection.TrueBitsEnumerator _trueBitsEnumerator;

            internal DirtyEnumerator(BitsCollection bitsCollection)
            {
                _trueBitsEnumerator = bitsCollection.GetEnumerator();
            }
            
            public void Reset() => _trueBitsEnumerator.Reset();

            public bool MoveNext() => _trueBitsEnumerator.MoveNext();

            public UnitId Current => new(_trueBitsEnumerator.Current);
            
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _trueBitsEnumerator.Dispose();
            }
        }
    }
}