using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OperationSystem.Shared;

namespace OperationSystem.Component.Types
{
    public readonly struct ComponentMask
    {
        private readonly BitsCollection _typeBits;
        
        private ComponentMask(BitsCollection typeBits)
        {
            _typeBits = typeBits;
        }
        
        public static ComponentMask Create(params int[] typeIds)
        {
            if (typeIds.Length == 0)
            {
                var emptyBits = BitsCollection.Create();
                return new ComponentMask(emptyBits);
            }

            var typeBits = BitsCollection.Create(typeIds.Max() + 1);
            
            var mask = new ComponentMask(typeBits);
            foreach (var id in typeIds) 
				mask.Add(id);
            return mask;
        }

        public static ComponentMask Create<T>() where T : struct, IComponent => Create(ComponentType<T>.Id);

        public bool Contains(int typeId) => _typeBits.IsTrue(typeId);
        public bool Contains<T>() where T : struct, IComponent => Contains(ComponentType<T>.Id);
        
        public void Add(int typeId) => _typeBits.SetTrue(typeId);
        public void Add<T>() where T : struct, IComponent => Add(ComponentType<T>.Id);


        public void Remove(int typeId) => _typeBits.SetFalse(typeId);
        public void Remove<T>() where T : struct, IComponent => Remove(ComponentType<T>.Id);

        public bool IsEmpty => !_typeBits.Any();

        public ComponentTypeEnumerator GetEnumerator() => new(_typeBits);
        
        public struct ComponentTypeEnumerator : IEnumerator<int>
        {
            private BitsCollection.TrueBitsEnumerator _trueBitsEnumerator;

            internal ComponentTypeEnumerator(BitsCollection bitsCollection)
            {
                _trueBitsEnumerator = bitsCollection.GetEnumerator();
            }
            
            public void Reset() => _trueBitsEnumerator.Reset();

            public bool MoveNext() => _trueBitsEnumerator.MoveNext();

            public int Current => _trueBitsEnumerator.Current;
            
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _trueBitsEnumerator.Dispose();
            }
        }
    }
}