using System.Collections.Generic;
using ECS.Components;
using UnityEngine;

namespace ECS.Examples
{
    public class MagazineAsset : MonoBehaviour,
                                 IAsset,
                                 IAssetPull<TransformComponent>,
                                 IAssetPush<TransformComponent>,
                                 IAssetPull<BufferComponent<ItemSlot>>,
                                 IAssetPush<BufferComponent<ItemSlot>>
    {
        public List<ItemSlot> ammoSlots = new();

        void IAssetPull<TransformComponent>.PullInto(ref TransformComponent c)
        {
            var t = transform;
            c.Position = t.position;
            c.Rotation = t.rotation;
            c.Scale = t.localScale;
        }

        void IAssetPush<TransformComponent>.PushFrom(in TransformComponent c)
        {
            var t = transform;
            t.position = c.Position;
            t.rotation = c.Rotation;
            t.localScale = c.Scale;
        }

        void IAssetPull<BufferComponent<ItemSlot>>.PullInto(ref BufferComponent<ItemSlot> c)
        {
            c.CopyFrom(ammoSlots.ToArray(), ammoSlots.Count);
        }

        void IAssetPush<BufferComponent<ItemSlot>>.PushFrom(in BufferComponent<ItemSlot> c)
        {
            ammoSlots.Clear();
            var span = c.AsSpan();
            for (var i = 0; i < span.Length; i++)
                ammoSlots.Add(span[i]);
        }
    }
}
