using System.Collections;
using System.Collections.Generic;
using ECS.Shared;
using OperationSystem.Units;

namespace ECS
{
    public readonly struct DirtyTracker
    {
        private readonly BitsCollection _dirtyBits;

        public DirtyTracker(int initialCapacity = 63)
        {
            _dirtyBits = new BitsCollection(initialCapacity);
        }

        public void SetDirty(UnitId unitId) => _dirtyBits.SetTrue(unitId.Id);
        public bool IsDirty(UnitId unitId) => _dirtyBits.IsTrue(unitId.Id);
        public void Clear(UnitId unitId) => _dirtyBits.SetFalse(unitId.Id);
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