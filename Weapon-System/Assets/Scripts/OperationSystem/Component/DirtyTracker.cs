using System.Collections.Generic;
using OperationSystem.Shared;

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

        public IEnumerator<int> GetEnumerator() => _dirtyBits.GetEnumerator();
    }
}