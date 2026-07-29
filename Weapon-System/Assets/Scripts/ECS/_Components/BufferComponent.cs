using System;
using System.Runtime.CompilerServices;

namespace ECS.Components
{
    public struct BufferComponent<T> : IComponent where T : struct
    {
        public T[] Items;
        public int Count;

        public BufferComponent(int capacity)
        {
            Items = new T[capacity];
            Count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => Items.AsSpan(0, Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref T GetElement(int index) => ref Items[index];

        public void Clear()
        {
            if (Items != null)
                Array.Clear(Items, 0, Count);
            Count = 0;
        }

        public void Add(T item)
        {
            if (Items == null || Count >= Items.Length)
            {
                var newSize = Items == null ? 4 : Items.Length * 2;
                Array.Resize(ref Items, newSize);
            }
            Items[Count++] = item;
        }

        public void CopyFrom(T[] source, int length)
        {
            if (Items == null || Items.Length < length)
                Items = new T[length];
            Array.Copy(source, Items, length);
            Count = length;
        }

        public void CopyTo(T[] destination)
        {
            if (Items != null)
                Array.Copy(Items, destination, Count);
        }
    }
}
